# Autenticação por Tokens (JWT em Cookie HttpOnly) — Guia Completo

> **Escopo.** Este documento descreve, de ponta a ponta, como funciona a autenticação do sistema *Gestão de Turmas*: um **JWT** (access token) transportado dentro de um **cookie `HttpOnly`**, acompanhado de um **refresh token** (também em cookie) para renovar a sessão sem novo login. Cobre desde a configuração no `Program.cs` até a forma como o `AuthController` recebe e devolve os dados, como os tokens são gerados, validados e salvos, e por que cada método existe.
>
> **Público-alvo.** Alguém que acabou de entrar no projeto e precisa entender o fluxo inteiro. Cada seção traz uma parte de **implementação** (o que o código faz) e uma parte de **conceitos** (os fundamentos por trás).
>
> **Modelo escolhido (decisão de projeto):** refresh token **stateless** (JWT puro, sem persistência em banco). O trade-off está documentado na seção 7.

---

## Índice

1. [Visão geral do modelo](#1-visão-geral-do-modelo)
2. [Mapa dos arquivos](#2-mapa-dos-arquivos)
3. [Configuração no `Program.cs`](#3-configuração-no-programcs)
4. [Configuração do JWT no `appsettings.json`](#4-configuração-do-jwt-no-appsettingsjson)
5. [Geração e validação de tokens — `TokenService`](#5-geração-e-validação-de-tokens--tokenservice)
6. [Regras de sessão — `AuthService`](#6-regras-de-sessão--authservice)
7. [Como os tokens são salvos — os cookies](#7-como-os-tokens-são-salvos--os-cookies)
8. [`AuthController` — como recebe e devolve](#8-authcontroller--como-recebe-e-devolve)
9. [DTOs de transporte](#9-dtos-de-transporte)
10. [Autorização nos controllers protegidos](#10-autorização-nos-controllers-protegidos)
11. [Fluxos de interação do usuário](#11-fluxos-de-interação-do-usuário)
12. [Resumo acionável](#12-resumo-acionável)

---

## 1. Visão geral do modelo

```
┌─────────────────────────┐        HTTPS         ┌──────────────────────────────┐
│  Angular SPA            │   ◄──────────────►    │  API .NET 10 (GestãoDeTurmas)│
│  https://localhost:4200 │                       │  https://localhost:7048      │
│                         │   Cookies enviados     │                              │
│  - HttpClient           │   automaticamente:     │  - JwtBearer Handler         │
│    (withCredentials)    │   access_token         │    (lê o token do cookie)    │
│  - Interceptor de       │   refresh_token        │  - TokenService (gera/valida)│
│    refresh (single-     │                        │  - AuthService (credenciais) │
│    flight + retry)      │                        │  - [Authorize] por Role      │
└─────────────────────────┘                       └──────────────────────────────┘
```

O usuário faz **login** com e-mail e senha. Em vez de devolver o token no corpo da resposta (que obrigaria o front a guardá-lo em `localStorage`), a API grava dois **cookies `HttpOnly`**:

- **`access_token`** — JWT de vida curta (15 min). É a credencial usada em toda request protegida.
- **`refresh_token`** — JWT de vida longa (7 dias). Só serve para obter um novo `access_token` quando o atual expira.

O front **nunca lê** esses cookies (são `HttpOnly`); ele apenas confia que o navegador os envia sozinho em cada request (`withCredentials: true`).

### Conceitos

- **JWT (JSON Web Token).** Uma string `header.payload.signature`. O *payload* carrega *claims* (afirmações sobre o usuário: id, e-mail, role). A *signature* é um HMAC-SHA256 calculado com uma chave secreta do servidor. Qualquer alteração no payload invalida a assinatura — por isso o token é **auto-verificável**: o servidor confia nele sem consultar banco, só checando a assinatura e a expiração.
- **Stateless.** O servidor não guarda sessão nem token em lugar nenhum. Toda a informação necessária está dentro do próprio JWT. Vantagem: escala sem estado compartilhado. Custo: não dá para "revogar" um token antes de ele expirar (ver seção 7).
- **Access + Refresh.** O access token é curto para limitar o dano se vazar. Como pedir login a cada 15 min é inviável, o refresh token (longo) permite renovar silenciosamente.
- **Por que cookie `HttpOnly` e não `localStorage`?** Um cookie `HttpOnly` é invisível ao JavaScript, então um ataque de XSS não consegue roubar o token. Em `localStorage`, qualquer script (inclusive libs de terceiros) leria o token.

---

## 2. Mapa dos arquivos

| Arquivo | Responsabilidade |
|---|---|
| `GestãoDeTurmas/Program.cs` | Configura DI, DbContext, o handler JWT Bearer (que lê o cookie), CORS e a ordem do pipeline. |
| `GestãoDeTurmas/appsettings.json` | Parâmetros do JWT: `Issuer`, `Audience`, `ExpireMinutes`, `RefreshExpireDays`. |
| `GestãoDeTurmas/Controllers/AuthController.cs` | Endpoints `login`, `refresh`, `logout`, `me`. Escreve e remove os cookies. |
| `API/Service/TokenService.cs` | Gera o access e o refresh token; valida o refresh token. |
| `API/Service/AuthService.cs` | Valida credenciais no login; orquestra a renovação de sessão. |
| `API/Service/ITokenService.cs`, `IAuthService.cs` | Contratos dos serviços acima. |
| `API/DTOs/AuthDTOs/LoginInput.cs` | Corpo recebido no login (`Email`, `Senha`). |
| `API/DTOs/AuthDTOs/UsuarioPublicoOutput.cs` | Corpo devolvido (`Role`) — nunca expõe dados sensíveis. |
| `API/DTOs/AuthDTOs/RenovacaoSessaoResultado.cs` | Resultado interno do refresh (novos tokens + role). |
| `Common/Domains/Usuario.cs` | Entidade do usuário (`Id`, `Email`, `SenhaHash`, `Role`). |
| `Repository/Repositories/UsuarioRepository/` | Busca usuário por e-mail e por id. |
| `Repository/Mappings/UsuarioConfiguration.cs` | Mapeamento EF + constraint de `Role`. |

> Observação: os serviços e DTOs vivem no projeto de biblioteca `API`, mas quem **roda** a aplicação (com os controllers e o `Program.cs` ativo) é o projeto `GestãoDeTurmas`.

---

## 3. Configuração no `Program.cs`

### (A) Implementação

**Registro dos serviços de auth no contêiner de DI:**

```csharp
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

builder.Services.AddDbContext<GestaoEscolarContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Handler JWT Bearer que lê o token do cookie:**

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["access_token"];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
```

**CORS (para o Angular enviar cookies cross-origin):**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("https://localhost:4200")
              .AllowCredentials()
              .WithHeaders("Content-Type", "Accept", "X-XSRF-TOKEN")
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .WithExposedHeaders("X-XSRF-TOKEN");
    });
});
```

**Ordem do pipeline:**

```csharp
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("PermitirAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

### (B) Conceitos

#### Por que o token vem do cookie e não do header `Authorization`

Por padrão, o handler JWT Bearer procura o token no header `Authorization: Bearer <token>`. Mas o front **não seta esse header** — ele deixa o token no cookie `HttpOnly` (que o JavaScript nem consegue ler para copiar). O evento `OnMessageReceived` é o gancho que resolve isso: antes de validar, ele diz "o token está aqui, no cookie `access_token`". A partir daí, o handler valida esse valor normalmente.

#### O que cada parâmetro de validação faz

- **`ValidateIssuerSigningKey` + `IssuerSigningKey`** — confirma que a assinatura do JWT foi feita com a nossa chave secreta. É o que impede alguém de forjar um token.
- **`ValidateIssuer` / `ValidateAudience`** — confirma que os campos `iss` (quem emitiu) e `aud` (para quem) batem com o configurado. Evita que um token emitido por outro sistema seja aceito aqui.
- **`ValidateLifetime`** — rejeita tokens expirados. É isto que transforma um access token vencido em **401** (o gatilho do fluxo de refresh).
- **`ClockSkew = TimeSpan.Zero`** — por padrão o .NET tolera 5 minutos de diferença de relógio. Zerar essa folga faz a expiração de 15 min valer exatamente 15 min (importante quando o access token já é curto).

#### Resultado da autenticação bem-sucedida

Se o cookie contém um JWT válido, o handler reidrata um `ClaimsPrincipal` e o coloca em `HttpContext.User`. Daí em diante, dentro de qualquer controller, `User.FindFirstValue(ClaimTypes.Role)` e `[Authorize(Roles = "...")]` funcionam.

#### Por que a ordem do pipeline importa

Middlewares executam na ordem em que são registrados. `UseAuthentication` **precisa** vir antes de `UseAuthorization`: o primeiro lê o cookie e descobre quem é o usuário; o segundo decide se aquele usuário pode acessar a rota. Sem `UseAuthentication`, o `User` seria sempre anônimo e tudo protegido responderia 401 mesmo após login. `UseCors` vem antes da autenticação porque o *preflight* `OPTIONS` do navegador é anônimo e não deve ser barrado.

#### `AllowCredentials()` + origem explícita

Cookies só viajam cross-origin se o servidor declarar `AllowCredentials()` **e** a origem for explícita (a spec proíbe o coringa `*` junto com credenciais). Front (`localhost:4200`) e API (`localhost:7048`) diferem só na porta, mas o navegador trata `localhost` como o mesmo *site*, então `SameSite=Lax` deixa o cookie passar.

#### Seed do administrador

No boot, o `Program.cs` cria um usuário `admin@admin.com` (senha `Admin123!`, role `Admin`) se a tabela `Usuarios` estiver vazia — garante que exista alguém para logar em ambiente novo.

---

## 4. Configuração do JWT no `appsettings.json`

### (A) Implementação

```json
"Jwt": {
  "Issuer": "GestaoTurmas",
  "Audience": "GestaoTurmas.Front",
  "ExpireMinutes": 15,
  "RefreshExpireDays": 7
}
```

### (B) Conceitos

- **`Issuer` / `Audience`** — identificam quem emite e para quem o token vale; conferidos na validação.
- **`ExpireMinutes` (15)** — validade do access token. Curto de propósito.
- **`RefreshExpireDays` (7)** — validade do refresh token. Define por quanto tempo o usuário pode ficar sem refazer login.
- **`Jwt:Key`** — a chave secreta de assinatura. **Não está neste arquivo** (versionado): fica em *user secrets* / variável de ambiente, para não vazar no repositório. O `TokenService` e o handler leem via `builder.Configuration["Jwt:Key"]`.

---

## 5. Geração e validação de tokens — `TokenService`

### (A) Implementação

```csharp
public class TokenService : ITokenService
{
    private const string ClaimTipoToken = "token_type";
    private const string TipoAccess = "access";
    private const string TipoRefresh = "refresh";

    private readonly IConfiguration _config;

    public TokenService(IConfiguration config) => _config = config;

    public string GerarToken(Usuario usuario)
        => GerarJwt(usuario, TipoAccess, TimeSpan.FromMinutes(MinutosAccess));

    public string GerarRefreshToken(Usuario usuario)
        => GerarJwt(usuario, TipoRefresh, TimeSpan.FromDays(DiasRefresh));

    public ClaimsPrincipal? ValidarRefreshToken(string token)
    {
        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, ParametrosValidacao(), out _);

            return principal.FindFirstValue(ClaimTipoToken) == TipoRefresh
                ? principal
                : null;
        }
        catch
        {
            return null;
        }
    }

    private string GerarJwt(Usuario usuario, string tipoToken, TimeSpan duracao)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Role),
            new Claim(ClaimTipoToken, tipoToken),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(duracao),
            signingCredentials: new SigningCredentials(Chave(), SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    // ParametrosValidacao(), Chave(), MinutosAccess, DiasRefresh: helpers privados
}
```

### (B) Conceitos e por que cada método existe

#### `GerarToken` e `GerarRefreshToken`

Ambos delegam a um helper privado `GerarJwt`, mudando só o **tipo** e a **duração**. Isso evita duplicar a montagem de claims e assinatura. O access dura minutos; o refresh dura dias.

#### O helper `GerarJwt` e as claims

Cada token embute quatro claims:

- **`NameIdentifier`** = o id do usuário. É o que o refresh usa para reencontrar o usuário no banco.
- **`Email`** e **`Role`** = dados de identidade/autorização. A `Role` é o que os `[Authorize(Roles = ...)]` consultam.
- **`token_type`** (`access` ou `refresh`) — claim customizada, explicada abaixo.

A assinatura usa **HMAC-SHA256** com a chave secreta: o mesmo segredo assina e verifica (criptografia simétrica).

#### `ValidarRefreshToken` — e por que a claim `token_type` existe

O refresh token é validado **manualmente** aqui (o handler JWT do pipeline valida só o access, que vem do cookie `access_token`). A validação:

1. Confere assinatura, issuer, audience e expiração (mesmos parâmetros do pipeline).
2. **Confere que `token_type == "refresh"`.**

O passo 2 é a razão de existir a claim `token_type`. Sem ela, como access e refresh são assinados com a mesma chave, alguém poderia mandar um **access token** no lugar do refresh e ele passaria na validação de assinatura. A claim garante que só um token realmente emitido como refresh é aceito no `/refresh`. Qualquer falha (assinatura inválida, expirado, tipo errado) cai no `catch` e retorna `null` — que o controller traduz em **401**.

---

## 6. Regras de sessão — `AuthService`

### (A) Implementação

```csharp
public async Task<Usuario?> ValidarCredenciaisAsync(string email, string senha)
{
    var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
    if (usuario is null) return null;
    var resultado = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
    return resultado == PasswordVerificationResult.Success ? usuario : null;
}

public async Task<RenovacaoSessaoResultado?> RenovarSessaoAsync(string? refreshToken)
{
    if (string.IsNullOrWhiteSpace(refreshToken))
        return null;

    var principal = _tokenService.ValidarRefreshToken(refreshToken);
    if (principal is null)
        return null;

    if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
        return null;

    var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
    if (usuario is null)
        return null;

    return new RenovacaoSessaoResultado(
        _tokenService.GerarToken(usuario),
        _tokenService.GerarRefreshToken(usuario),
        usuario.Role);
}
```

### (B) Conceitos e por que cada método existe

#### `ValidarCredenciaisAsync` — o login de verdade

Busca o usuário pelo e-mail e compara a senha usando `IPasswordHasher<Usuario>`. A senha **nunca** é comparada em texto puro: o banco guarda só o `SenhaHash`, e o `VerifyHashedPassword` recalcula o hash da senha digitada e compara. Retorna o usuário em caso de sucesso, ou `null` (que vira 401 no controller). Note que a resposta é a mesma para "e-mail não existe" e "senha errada" — não vaza qual dos dois falhou.

#### `RenovarSessaoAsync` — o coração do refresh

Recebe o refresh token (que veio do cookie) e:

1. **Rejeita cedo** se vier vazio/nulo.
2. **Valida** o token via `TokenService` (assinatura, expiração, `token_type`).
3. **Extrai o id** do usuário da claim `NameIdentifier`.
4. **Recarrega o usuário do banco** (`ObterPorIdAsync`).
5. Emite **novos** access e refresh tokens e devolve junto a role atual.

##### Por que recarregar o usuário do banco (mesmo sendo stateless)?

Poderíamos confiar só nas claims do refresh token. Recarregar do banco custa uma consulta, mas garante que:

- Um usuário **deletado** não consiga renovar a sessão (o token ainda é válido criptograficamente, mas não existe mais no banco → `null` → 401).
- A **role usada no novo token seja a atual** — se o papel do usuário mudou desde o login, o novo access token reflete a mudança.

##### Por que retornar um objeto e não escrever cookies aqui

O `AuthService` não conhece `HttpContext` nem cookies (é uma camada de serviço, sem dependência de web). Ele devolve um `RenovacaoSessaoResultado` (tokens + role) e deixa a escrita de cookies para o controller, que é quem tem acesso à `Response`. Separação de responsabilidades.

---

## 7. Como os tokens são salvos — os cookies

### (A) Implementação (helpers do `AuthController`)

```csharp
private const string CookieAccess = "access_token";
private const string CookieRefresh = "refresh_token";
private const string CaminhoRefresh = "/api/auth";

private void DefinirCookieAcesso(string token) =>
    Response.Cookies.Append(CookieAccess, token, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddMinutes(15),
    });

private void DefinirCookieRefresh(string token) =>
    Response.Cookies.Append(CookieRefresh, token, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Path = CaminhoRefresh,
        Expires = DateTimeOffset.UtcNow.AddDays(7),
    });

private void RemoverCookiesSessao()
{
    Response.Cookies.Delete(CookieAccess, new CookieOptions
    {
        HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax,
    });
    Response.Cookies.Delete(CookieRefresh, new CookieOptions
    {
        HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Path = CaminhoRefresh,
    });
}
```

### (B) Conceitos

#### O que cada flag do cookie faz

- **`HttpOnly = true`** — esconde o cookie do JavaScript (`document.cookie` não o vê). É o pilar de segurança: XSS não rouba o token.
- **`Secure = true`** — o cookie só trafega em HTTPS. Por isso a API roda em HTTPS.
- **`SameSite = Lax`** — o cookie é enviado em requisições *same-site*. Como front e API são ambos `localhost`, contam como same-site e o cookie passa; ao mesmo tempo, bloqueia a maior parte dos vetores CSRF cross-site.
- **`Expires`** — quando o navegador descarta o cookie. Casado com a expiração do token (15 min / 7 dias).

#### Por que o `refresh_token` tem `Path = /api/auth`

O access token vai em `Path = /` (default), pois precisa acompanhar **toda** request protegida (`/api/turmas`, `/api/docentes`, ...). Já o refresh token só é usado no endpoint `/api/auth/refresh`. Restringindo o `Path` a `/api/auth`, o navegador só envia esse cookie de vida longa nas rotas de auth — **menos exposição** do token mais sensível. A remoção no logout usa o **mesmo `Path`**, senão o navegador não casaria o cookie para apagá-lo.

#### Como o logout funciona

Não existe "apagar cookie" no HTTP — o servidor manda um `Set-Cookie` com data de expiração no passado, e o navegador descarta. É o que `Response.Cookies.Delete` faz para os dois cookies.

#### Trade-off do modelo stateless (importante)

Como não há persistência de tokens, **não há como revogar um refresh token antes de ele expirar**. O logout apaga o cookie no navegador do usuário, mas se um refresh token tivesse sido copiado antes, ele continuaria válido até os 7 dias acabarem. Foi a decisão consciente do projeto (simplicidade, sem schema novo). Se um dia for preciso "sair de todos os dispositivos" ou revogação imediata, seria necessário migrar para refresh tokens **persistidos** (com tabela no banco).

Um efeito colateral **positivo** do stateless: como o refresh não invalida o token anterior ao rotacionar, dois refreshes quase simultâneos (a janela de corrida que o front tenta evitar com *single-flight*) ambos funcionam — não há risco de deslogar um usuário legítimo.

---

## 8. `AuthController` — como recebe e devolve

### (A) Implementação

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginInput input)
    {
        var usuario = await _authService.ValidarCredenciaisAsync(input.Email, input.Senha);
        if (usuario is null) return Unauthorized();

        DefinirCookieAcesso(_tokenService.GerarToken(usuario));
        DefinirCookieRefresh(_tokenService.GerarRefreshToken(usuario));
        return Ok(new UsuarioPublicoOutput(usuario.Role));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var resultado = await _authService.RenovarSessaoAsync(Request.Cookies[CookieRefresh]);
        if (resultado is null) return Unauthorized();

        DefinirCookieAcesso(resultado.AccessToken);
        DefinirCookieRefresh(resultado.RefreshToken);
        return Ok(new UsuarioPublicoOutput(resultado.Role));
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        RemoverCookiesSessao();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role is null) return Forbid();
        return Ok(new UsuarioPublicoOutput(role));
    }
}
```

### (B) Conceitos — por que cada endpoint existe e o que devolve

#### `POST /api/auth/login` — `[AllowAnonymous]`

Recebe `{ email, senha }` no corpo (`[FromBody]`). Valida via `AuthService`. Em caso de sucesso, **grava os dois cookies** e devolve `200 OK` com `{ "role": "..." }`. Em falha, `401` sem cookie. É anônimo porque o usuário ainda não tem sessão.

- **Por que devolver só a `role`?** O front precisa saber o papel para montar o menu/rotas, mas não precisa (e não deve) receber o token no corpo — ele está no cookie. Nada sensível é exposto.

#### `POST /api/auth/refresh` — `[AllowAnonymous]`

**Não recebe corpo.** Lê o `refresh_token` do cookie e chama `RenovarSessaoAsync`. Sucesso → grava cookies novos + `200 { role }`. Falha → `401` sem cookie.

- **Por que `[AllowAnonymous]`?** Quando o front chama `/refresh`, o access token já expirou (senão não precisaria renovar). Se este endpoint exigisse `[Authorize]`, ele responderia 401 sempre e a renovação nunca aconteceria. A credencial aqui é o refresh token do cookie, validado manualmente.
- **Anti-loop:** a URL contém `/auth/`, então o interceptor do front **não** re-tenta um refresh sobre a resposta deste endpoint. Se o refresh falha, a API só devolve um 401 limpo e o front manda o usuário ao login.

#### `POST /api/auth/logout` — `[Authorize]`

Expira os dois cookies e devolve `204 No Content`. Exige estar autenticado (faz sentido só para quem tem sessão).

#### `GET /api/auth/me` — `[Authorize]`

Lê a `role` das claims do `HttpContext.User` (populado pelo handler JWT) e devolve `{ role }`. Serve para o front, ao recarregar a página, descobrir se ainda há sessão válida e qual o papel. Se o access token estiver expirado/ausente, o pipeline responde `401` **antes** de chegar no método.

#### 401 vs 403 (o front depende disso)

- **401 Unauthorized** = "não sei quem você é" (sem token ou token expirado). Gatilho para o front tentar refresh e, se falhar, mandar ao login.
- **403 Forbidden** = "sei quem você é, mas você não tem permissão" (role insuficiente). O front trata como "acesso negado", **não** como fim de sessão. Por isso o `/refresh` nunca devolve 403 na falha — sempre 401.

---

## 9. DTOs de transporte

```csharp
public record LoginInput(string Email, string Senha);
public record UsuarioPublicoOutput(string Role);
public record RenovacaoSessaoResultado(string AccessToken, string RefreshToken, string Role);
```

### Conceitos

- **`LoginInput`** — só o necessário para autenticar. Chega no corpo do `login`.
- **`UsuarioPublicoOutput`** — o que **sai** da API para o front (`login`, `refresh`, `me`). Deliberadamente mínimo (só `Role`); nunca inclui token, hash de senha ou dados internos. O nome "Público" reforça que é seguro expor.
- **`RenovacaoSessaoResultado`** — DTO **interno** entre `AuthService` e `AuthController`; carrega os tokens novos para o controller escrever nos cookies. Nunca é serializado na resposta.

---

## 10. Autorização nos controllers protegidos

### (A) Implementação (ex.: `TurmasController`)

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Coordenador")]
public class TurmasController : ControllerBase { ... }
```

### (B) Conceitos

- **`[Authorize]`** sem parâmetros exige apenas um usuário autenticado (access token válido no cookie).
- **`[Authorize(Roles = "Admin,Coordenador")]`** exige, além disso, que a claim `Role` do token seja `Admin` **ou** `Coordenador`. É o mesmo `Role` que o `TokenService` embutiu no JWT no login.
- O fluxo é: `UseAuthentication` lê o cookie e popula `User` → `UseAuthorization` compara a `Role` do `User` com o exigido → deixa passar, ou short-circuita com 401 (sem token) / 403 (role insuficiente).

---

## 11. Fluxos de interação do usuário

Abaixo, quatro cenários ponta a ponta (o mínimo pedido são três). Cada um mostra o que o navegador envia, o que a API faz internamente e o que devolve.

### Fluxo A — Login bem-sucedido

**Situação:** usuário abre o app e faz login com `admin@admin.com` / `Admin123!`.

```
1. Front → API
   POST https://localhost:7048/api/auth/login
   Content-Type: application/json
   { "email": "admin@admin.com", "senha": "Admin123!" }

2. API (interno)
   AuthController.Login
     → AuthService.ValidarCredenciaisAsync
         → UsuarioRepository.ObterPorEmailAsync("admin@admin.com")  → Usuario
         → PasswordHasher.VerifyHashedPassword(...)                 → Success
     → TokenService.GerarToken(usuario)         → JWT access (exp 15min, token_type=access)
     → TokenService.GerarRefreshToken(usuario)  → JWT refresh (exp 7d, token_type=refresh)
     → grava os dois cookies

3. API → Front
   200 OK
   Set-Cookie: access_token=<jwt>;  HttpOnly; Secure; SameSite=Lax; Path=/;         Expires=+15min
   Set-Cookie: refresh_token=<jwt>; HttpOnly; Secure; SameSite=Lax; Path=/api/auth; Expires=+7d
   { "role": "Admin" }
```

O front guarda só a `role` (para montar a UI). Os tokens ficam nos cookies, fora do alcance do JavaScript.

### Fluxo B — Request autenticada com access token válido

**Situação:** logo após o login, o usuário lista turmas.

```
1. Front → API
   GET https://localhost:7048/api/turmas?pagina=1
   Cookie: access_token=<jwt>   (o navegador anexa sozinho, withCredentials)

2. API (interno)
   UseAuthentication → JwtBearer.OnMessageReceived pega o token do cookie access_token
                     → valida assinatura, issuer, audience, expiração → OK
                     → HttpContext.User recebe as claims (inclui Role=Admin)
   UseAuthorization  → [Authorize(Roles="Admin,Coordenador")] → Admin passa
   TurmasController.ObterTurmas executa

3. API → Front
   200 OK
   { "itens": {...}, "paginaAtual": 1, ... }
```

O `refresh_token` **não** é enviado aqui (o `Path=/api/auth` não casa com `/api/turmas`) — exatamente o desejado.

### Fluxo C — Access token expirado → refresh automático → retry

**Situação:** o usuário ficou 20 min inativo; o access token (15 min) expirou. Ele clica em algo que dispara um `GET /api/turmas`.

```
1. Front → API
   GET /api/turmas
   Cookie: access_token=<jwt expirado>

2. API → Front
   401 Unauthorized          (ValidateLifetime reprovou o token)

3. Front (interceptor) detecta 401 numa URL que NÃO contém /auth/
   → dispara UM refresh (single-flight) e segura as demais requests

   Front → API
   POST /api/auth/refresh
   Cookie: refresh_token=<jwt válido>   (enviado porque a URL casa com Path=/api/auth)

4. API (interno)
   AuthController.Refresh
     → AuthService.RenovarSessaoAsync(refreshToken)
         → TokenService.ValidarRefreshToken(...)  → OK (token_type=refresh, não expirou)
         → UsuarioRepository.ObterPorIdAsync(id)   → Usuario ainda existe
         → gera novo access + novo refresh
     → grava cookies novos

   API → Front
   200 OK
   Set-Cookie: access_token=<novo jwt>;  ... Expires=+15min
   Set-Cookie: refresh_token=<novo jwt>; ... Expires=+7d
   { "role": "Admin" }

5. Front re-tenta a request original, agora com o access token novo
   GET /api/turmas → 200 OK
```

Tudo isso é transparente para o usuário: ele só percebe que a tela carregou normalmente.

### Fluxo D — Refresh token inválido/expirado → login

**Situação:** o usuário voltou depois de 8 dias; o refresh token (7 dias) expirou. Alguma request devolve 401 e o front tenta refresh.

```
1. Front → API
   POST /api/auth/refresh
   Cookie: refresh_token=<jwt expirado>

2. API (interno)
   AuthController.Refresh
     → AuthService.RenovarSessaoAsync
         → TokenService.ValidarRefreshToken(...)  → catch (expirado) → null
     → resultado null

3. API → Front
   401 Unauthorized
   (NENHUM Set-Cookie — a sessão não foi renovada)

4. Front: como o /refresh (URL contém /auth/) respondeu 401,
   o interceptor NÃO tenta outro refresh (anti-loop) → redireciona para a tela de login.
```

Mesma resposta (401 sem cookie) vale para refresh ausente, assinatura inválida, `token_type` errado ou usuário que não existe mais no banco.

### Fluxo E — Logout

**Situação:** o usuário clica em "Sair".

```
1. Front → API
   POST /api/auth/logout
   Cookie: access_token=<jwt válido>

2. API (interno)
   UseAuthentication/Authorization → [Authorize] OK
   AuthController.Logout → RemoverCookiesSessao() → Set-Cookie dos dois no passado

3. API → Front
   204 No Content
   Set-Cookie: access_token=;  Expires=(passado)  → navegador descarta
   Set-Cookie: refresh_token=; Path=/api/auth; Expires=(passado) → navegador descarta

4. Front limpa o estado local (role) e vai para o login.
```

Lembrando o trade-off da seção 7: o logout limpa os cookies **deste navegador**; no modelo stateless, tokens que porventura tenham sido copiados antes continuam válidos até expirarem.

---

## 12. Resumo acionável

- **Login** grava `access_token` (15 min, `Path=/`) e `refresh_token` (7 dias, `Path=/api/auth`), ambos `HttpOnly`+`Secure`+`SameSite=Lax`; devolve `{ role }`.
- **Toda request protegida** manda o `access_token` automaticamente; o handler JWT lê o cookie via `OnMessageReceived` e popula `HttpContext.User`.
- **Access expirado → 401.** O front faz `POST /auth/refresh` (single-flight) e re-tenta a request.
- **`/refresh`** é `[AllowAnonymous]`, lê o refresh do cookie, revalida contra o banco, rotaciona os dois tokens; falha = **401 sem cookie** (nunca 403).
- **Logout** expira os dois cookies (`204`).
- **Autorização** por `Role` embutida no JWT (`[Authorize(Roles = "...")]`).
- **Segredo `Jwt:Key`** fica em user-secrets/ambiente, fora do repositório.
- **Modelo stateless:** simples e sem schema novo, mas sem revogação de token antes da expiração.
```
