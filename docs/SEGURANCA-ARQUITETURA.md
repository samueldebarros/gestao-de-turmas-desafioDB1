# Arquitetura de Segurança — Backend .NET + Front-end Angular

> **Escopo do documento.** Este guia descreve, com profundidade, a reestruturação de segurança do sistema *Gestão de Turmas*. O front-end Angular (`http://localhost:4200`) consome a API .NET 10 e a autenticação é baseada em **Cookies HttpOnly** (sessão) com **proteção CSRF via Double-Submit Cookie** (`XSRF-TOKEN` / `X-XSRF-TOKEN`).
>
> Cada seção contém duas partes: **(A) Implementação** — código pronto para colar — e **(B) Conceitos Detalhados** — fundamentos teóricos, comportamento interno do framework, justificativas das decisões e alternativas viáveis.

---

## Índice

1. [Visão geral da arquitetura](#0-visão-geral-da-arquitetura)
2. [Diagnóstico do estado atual do repositório](#diagnóstico-do-estado-atual-do-repositório)
3. [CORS — liberar Angular com credenciais](#1-cors--liberar-angular-com-credenciais)
4. [Middleware de Cookies — emissão e leitura](#2-middleware-de-cookies--emissão-e-leitura)
5. [Antiforgery — Double-Submit Cookie (proteção CSRF)](#3-antiforgery--double-submit-cookie-proteção-csrf)
6. [Pipeline final do `Program.cs`](#4-pipeline-final-do-programcs)
7. [`AuthController` — `login`, `logout`, `me`](#5-authcontroller--login-logout-me)
8. [Autorização nos controllers (Roles e Policies)](#6-autorização-nos-controllers-roles-e-policies)
9. [Ajuste no `GlobalExceptionHandler`](#7-ajuste-no-globalexceptionhandler)
10. [Configuração esperada no Angular](#8-configuração-esperada-no-angular)
11. [Fluxo end-to-end](#9-fluxo-end-to-end)
12. [Checklist de implementação](#10-checklist-de-implementação)

---

## 0. Visão geral da arquitetura

```
┌────────────────────────┐      HTTPS       ┌──────────────────────────┐
│  Angular SPA           │  ◄────────────►  │  API .NET 10             │
│  http://localhost:4200 │                  │  https://localhost:5xxx  │
│                        │                  │                          │
│  - HttpClient          │   Cookies:       │  - CookieAuth Handler    │
│  - XSRF Interceptor    │   GestaoTurmas.Auth (HttpOnly, Secure, Lax) │
│  - AuthService         │   XSRF-TOKEN        (Secure, Lax)            │
│  - AuthGuard           │                  │                          │
│                        │   Headers:       │  - Antiforgery service   │
│                        │   X-XSRF-TOKEN   │  - [Authorize] policies  │
└────────────────────────┘                  └──────────────────────────┘
```

**Por que esse modelo (Cookie + CSRF) e não JWT em `localStorage`?**

| Critério | Cookie HttpOnly + CSRF | JWT em `localStorage` + `Authorization: Bearer` |
|---|---|---|
| Vulnerabilidade a XSS | Token **não acessível ao JS** → roubo de sessão é muito mais difícil | Token acessível por qualquer script (incluindo libs de terceiros) → XSS = sessão roubada |
| Vulnerabilidade a CSRF | Existe risco nativo (browser envia cookie automaticamente) → **precisa de defesa explícita** (este guia) | Imune por construção (browser não envia `Authorization` sozinho) |
| Logout server-side | Trivial (`SignOutAsync` ou invalidar sessão server) | Requer denylist, rotação ou tokens curtos |
| Mobile / outras origens | Mais atrito (cookies third-party) | Mais simples |
| Padrão OWASP recomendado para SPAs | ✅ Indicado quando a SPA está no mesmo domínio raiz | Aceito, mas com mitigações |

A escolha aqui privilegia **mitigar o pior cenário (XSS)** porque um único pacote npm malicioso pode comprometer todos os tokens em `localStorage`, enquanto um cookie `HttpOnly` resiste a esse tipo de comprometimento.

---

## Diagnóstico do estado atual do repositório

| Item | Arquivo | Estado |
|---|---|---|
| CORS | `GestãoDeTurmas/Program.cs:45-54` | Configurado, mas com ajustes pendentes |
| Cookie Auth | `GestãoDeTurmas/Program.cs:56-77` | Configurado, **mas falta `UseAuthentication` no pipeline** |
| Antiforgery (CSRF) | — | **Inexistente** |
| AuthController | `GestãoDeTurmas/Controllers/AuthController.cs:1-13` | **Stub vazio** retornando `View()` em um `[ApiController]` |
| `UseAuthentication()` | `Program.cs:97` | **Faltando** — só há `UseAuthorization()` (bug crítico: cookies não são lidos) |
| `MapControllers()` | `Program.cs:101-104` | Apenas rota MVC clássica; controllers `[ApiController]` não estão sendo mapeados |
| `GlobalExceptionHandler` | `Middlewares/GlobalExceptionHandler.cs:22-27` | Faz `Redirect` — quebra clientes Angular (deveriam receber JSON `ProblemDetails`) |
| Entidade `Docente` | `Common/Domains/Docente.cs:3-13` | **Sem `SenhaHash` nem `Role`** — necessários para login e autorização |

---

## 1. CORS — liberar Angular com credenciais

### (A) Implementação

Substitua `GestãoDeTurmas/Program.cs:45-54`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowCredentials()
              .WithHeaders("Content-Type", "Accept", "X-XSRF-TOKEN", "Authorization")
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .WithExposedHeaders("X-XSRF-TOKEN");
    });
});
```

### (B) Conceitos Detalhados

#### O que é CORS e por que ele existe

O navegador implementa a **Same-Origin Policy (SOP)**: um documento carregado de `http://localhost:4200` **não pode** ler respostas de `https://localhost:5001` por padrão. "Origem" aqui significa a tupla `(esquema, host, porta)` — basta uma das três diferir e as origens são distintas.

A SOP existe para impedir que um site malicioso (`evil.com`) carregue, em background, dados privados do seu webmail. Mas APIs públicas legítimas precisam permitir consumo cross-origin — daí o protocolo **CORS (Cross-Origin Resource Sharing)**, definido na especificação Fetch. O servidor responde com headers `Access-Control-*` declarando explicitamente quem pode ler suas respostas; o navegador é quem **bloqueia** quando a resposta não traz esses headers. O servidor sempre processa a requisição — o filtro é client-side.

#### O preflight `OPTIONS`

Para requisições "não simples" (qualquer uma com `Content-Type: application/json`, métodos `PUT`/`PATCH`/`DELETE`, ou headers customizados como `X-XSRF-TOKEN`), o navegador envia um **preflight** automático antes da requisição real:

```
OPTIONS /api/alunos HTTP/1.1
Origin: http://localhost:4200
Access-Control-Request-Method: POST
Access-Control-Request-Headers: content-type, x-xsrf-token
```

Se o servidor responder com headers que aceitem esse conjunto, o navegador então dispara o `POST` real. Se faltar qualquer item, ele **aborta sem enviar a requisição mutante** — e o erro aparece no console como "CORS error" sem nenhuma chamada visível no Network. Esse é o motivo de a maior parte dos bugs CORS aparecerem em verbos não-`GET`.

#### `AllowCredentials()`

Este método adiciona `Access-Control-Allow-Credentials: true` à resposta. Ele tem duas implicações:

1. O navegador **só envia cookies (e cabeçalhos `Authorization`)** em requisições cross-origin se o `fetch` foi chamado com `credentials: 'include'` **E** se o servidor declarou `Allow-Credentials: true`. Sem qualquer um dos dois, os cookies somem.
2. **O navegador rejeita `Access-Control-Allow-Origin: *` quando credenciais estão envolvidas.** Por isso `AllowAnyOrigin().AllowCredentials()` lança exceção em runtime no .NET: a spec proíbe o coringa nesse caso para evitar que qualquer site malicioso receba cookies de qualquer API.

#### `WithOrigins("http://localhost:4200")`

A origem precisa ser **a string exata** com esquema e porta. Não vale wildcard, nem regex, nem `localhost` sem porta. Em produção, troque para `https://app.empresa.com`. Para múltiplas origens (staging, prod), passe um array: `WithOrigins("https://app.empresa.com", "https://staging.empresa.com")`.

**Alternativa**: para dev, o Angular CLI permite configurar um **proxy** (`proxy.conf.json`) que faz o front e o back parecerem da mesma origem, evitando CORS completamente. É útil se o time não quer lidar com CORS no setup local, mas você ainda precisa configurá-lo para produção.

#### `WithHeaders(...)` — o que cada um significa

Esse método produz `Access-Control-Allow-Headers: Content-Type, Accept, X-XSRF-TOKEN, Authorization`. Sem ele, o preflight falha se a requisição real trouxer qualquer um desses cabeçalhos.

- **`Content-Type`** — sempre necessário em POST/PUT/PATCH com JSON. Ironicamente, `Content-Type: application/json` é considerado "não simples" justamente porque dispara o preflight; valores como `text/plain` ou `application/x-www-form-urlencoded` são "simples" e dispensam preflight, mas quebram o binding do .NET.
- **`Accept`** — opcional, mas o Angular `HttpClient` adiciona automaticamente. Sem ele na allowlist, alguns ambientes do navegador disparam preflight extras.
- **`X-XSRF-TOKEN`** — header customizado que o Angular envia em toda requisição mutante (POST/PUT/PATCH/DELETE) com o token CSRF. **Obrigatório** na lista, senão o preflight bloqueia qualquer mutação.
- **`Authorization`** — incluí na allowlist por precaução futura (se você decidir adicionar Basic Auth para alguma rota de admin ou autenticação machine-to-machine com bearer). Pode remover se nunca for usar.

**Por que não `AllowAnyHeader()`?** Funciona, mas é mais permissivo do que o necessário. O princípio do mínimo privilégio sugere listar apenas o que você realmente usa — qualquer header novo passa a exigir uma alteração explícita aqui, o que ajuda em revisões de segurança.

#### `WithMethods(...)`

Equivale a `Access-Control-Allow-Methods`. Sem ele, o preflight falha para qualquer verbo diferente de `GET`/`HEAD`/`POST` (os "simples"). `OPTIONS` precisa estar na lista porque o próprio preflight é um `OPTIONS` — embora o middleware do .NET geralmente já o trate antes.

#### `WithExposedHeaders("X-XSRF-TOKEN")`

Diferente do `Allow-Headers` (que controla o que o cliente pode **enviar**), o `Expose-Headers` (`Access-Control-Expose-Headers`) controla o que o JS pode **ler** em respostas cross-origin. Por padrão, o navegador só permite que o JS leia uma lista mínima (`Cache-Control`, `Content-Language`, `Content-Length`, `Content-Type`, `Expires`, `Last-Modified`, `Pragma`).

Se você quiser que o front consiga ler o token CSRF de um header de resposta (cenário opcional do middleware `AntiforgeryCookieMiddleware`, seção 3), precisa expor `X-XSRF-TOKEN`. **O cookie `XSRF-TOKEN` é lido independentemente disso** — `withExposedHeaders` afeta apenas leitura de *headers* (não cookies).

#### Como o Angular se comunica com essa configuração

No Angular 18+:

```typescript
// app.config.ts
provideHttpClient(
  withFetch(),
  withXsrfConfiguration({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' }),
  withInterceptors([credentialsInterceptor])
)

// credentials.interceptor.ts
export const credentialsInterceptor: HttpInterceptorFn = (req, next) =>
  next(req.clone({ withCredentials: true }));
```

Com isso, **todo** request do `HttpClient` é clonado com `withCredentials: true`, o que diz ao navegador "envie cookies cross-origin neste request". Sem esse `withCredentials`, o navegador descarta os cookies silenciosamente — bug clássico em dev: "fiz login e a próxima request não autenticou".

#### Alternativas

- **Proxy do Angular CLI** (`proxy.conf.json`): elimina CORS em dev, mas exige configuração extra em produção (Nginx/IIS reverse proxy). Útil em times que querem zerar a fricção do dev.
- **Servir Angular pela própria API** (`app.UseStaticFiles()` + `app.MapFallbackToFile("index.html")`): elimina CORS por completo já que tudo vive na mesma origem. Custo: você acopla deploy do front e do back. Recomendado quando a SPA é pequena e o time tem um único pipeline.
- **`AllowAnyOrigin()` sem credenciais + JWT no header `Authorization`**: usado quando a API é pública e clientes vêm de qualquer origem (apps mobile, integrações). Aqui o problema CORS desaparece porque os cookies somem do quadro, mas você troca por toda a complexidade de gestão de tokens.

---

## 2. Middleware de Cookies — emissão e leitura

### (A) Implementação

Substitua `GestãoDeTurmas/Program.cs:56-77`:

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name        = "GestaoTurmas.Auth";
        options.Cookie.HttpOnly    = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite    = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;

        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
```

### (B) Conceitos Detalhados

#### O que é um cookie e quem o envia

Cookie é um par chave-valor que o servidor grava no navegador via cabeçalho `Set-Cookie`. O navegador, **automaticamente**, anexa esse cookie em todas as requisições subsequentes para o mesmo domínio, no header `Cookie`. Não há JavaScript envolvido — é nativo do protocolo HTTP (RFC 6265).

É exatamente esse envio automático que torna cookies poderosos (não preciso lembrar de mandar nada) e perigosos (CSRF se aproveita disso, daí a seção 3).

#### Atributos do cookie e o que cada um faz

- **`HttpOnly = true`** — instrui o navegador a **esconder** este cookie do JavaScript. `document.cookie` simplesmente não o vê. Significado prático: se um atacante injetar JS na sua página (XSS), ele não consegue ler o cookie de sessão. **Esse é o pilar central da escolha "Cookie em vez de localStorage"**.

- **`Secure = true`** (via `CookieSecurePolicy.Always`) — o cookie só é enviado em conexões HTTPS. Em HTTP plano, o browser o omite. Se você esquecer disso em produção, qualquer Wi-Fi público consegue interceptar a sessão. O .NET tem três opções: `Always`, `None` (envia em qualquer), `SameAsRequest` (segue o esquema do request — péssima ideia em produção).

- **`SameSite`** — define se o cookie é enviado em requisições **cross-site**:
  - `Strict`: só é enviado quando a navegação se origina do próprio site. Mais seguro, mas quebra fluxos comuns (clicar num link de e-mail e já estar logado).
  - `Lax` (padrão de browsers modernos): é enviado em navegações top-level GET (clicar em link), mas **não** em sub-requests cross-site (iframe, imagem, `fetch`/`form POST`). Bloqueia a maior parte dos CSRFs.
  - `None`: enviado sempre, inclusive cross-site. Requer `Secure=true`.

  **Por que `Lax` aqui e não `None` ou `Strict`?**

  - O Angular roda em `http://localhost:4200` e a API em `https://localhost:5xxx`. Os dois compartilham o mesmo *eTLD+1* (`localhost`), então o browser os trata como **same-site** — `Lax` permite que o cookie viaje normalmente. Em produção, se o front estiver em `app.empresa.com` e a API em `api.empresa.com`, ambos têm `empresa.com` como eTLD+1 e continuam same-site.
  - `Strict` quebraria fluxos como "voltar para o app via link" (o cookie sumiria nesse primeiro request).
  - `None` é necessário apenas quando front e API ficam em domínios completamente distintos (`app.com` e `api.outraempresa.com`), o que não é o caso. Usar `None` aumenta a superfície CSRF: como qualquer site pode disparar uma requisição que carregue o cookie, a defesa CSRF passa a ser a única barreira (em vez de duas).

- **`IsEssential = true`** — bypass para o middleware GDPR de consentimento de cookies (`CookiePolicyMiddleware`). Sem essa flag, se o usuário não consentiu, o ASP.NET **não escreve o cookie**. Como o cookie de autenticação é essencial para o site funcionar, ele se enquadra na exceção legal e pode ser gravado sem consentimento explícito.

#### Como funciona internamente o `AddCookie`

O `CookieAuthenticationHandler` é registrado como um *AuthenticationScheme*. O que acontece em runtime:

1. **No `SignInAsync`** (chamado no endpoint de login):
   - Recebe um `ClaimsPrincipal` (a identidade do usuário).
   - Envolve em um `AuthenticationTicket` (que inclui a hora de expiração e propriedades).
   - Chama `IDataProtector.Protect(ticket-bytes)` — o `IDataProtector` é parte da **Data Protection API**, sistema do ASP.NET que gerencia chaves de criptografia automaticamente (rotação a cada 90 dias por padrão).
   - Grava o resultado cifrado no `Set-Cookie` com os flags configurados acima.

2. **Em toda requisição subsequente** (quando o `UseAuthentication` está no pipeline):
   - Lê o cookie.
   - Chama `IDataProtector.Unprotect(...)` — falha silenciosa se a chave foi rotacionada e expirou (usuário recebe 401 e refaz login).
   - Reidrata o `ClaimsPrincipal` e o coloca em `HttpContext.User`.

Resultado: dentro de qualquer controller, `User.Identity.IsAuthenticated`, `User.IsInRole("Admin")`, `User.FindFirstValue(ClaimTypes.NameIdentifier)` funcionam transparentemente.

**Ponto crítico**: a Data Protection API armazena chaves no sistema de arquivos local por padrão. Em ambientes com múltiplas instâncias (load balancer, container), você precisa configurar um repositório compartilhado (`PersistKeysToFileSystem` apontando para volume compartilhado, ou `PersistKeysToAzureBlobStorage`, ou `PersistKeysToStackExchangeRedis`), senão um cookie emitido pela instância A é rejeitado pela instância B.

#### `ExpireTimeSpan` + `SlidingExpiration`

- `ExpireTimeSpan = 8h` define a validade absoluta inicial do ticket.
- `SlidingExpiration = true` faz o handler renovar o cookie sempre que mais da metade do prazo já passou (≥4h, no caso). Sem isso, o usuário é deslogado às 8h da manhã se logou às 0h, mesmo estando ativo o dia todo.

A renovação só acontece se o pipeline efetivamente passar pelo `UseAuthentication` — outro motivo para a ordem dos middlewares estar correta.

#### Por que sobrescrever `OnRedirectToLogin` e `OnRedirectToAccessDenied`

O `CookieAuthenticationHandler` foi pensado originalmente para apps MVC server-side: quando o usuário não autenticado tenta uma rota `[Authorize]`, ele faz `302 Redirect → /Account/Login`. Para uma SPA, esse 302 é desastroso:

- O `fetch` segue o redirect (por padrão), recebe HTML da página de login, e o Angular tenta parsear como JSON → erro de parse.
- O usuário não vê nada, o store fica corrompido.

Sobrescrevendo esses eventos para devolver `401`/`403` puros, a SPA pode interceptar globalmente (`HttpInterceptor` no Angular) e disparar o redirect client-side.

#### Alternativas

- **JWT em header `Authorization: Bearer`** (sem cookies): elimina CSRF, mas exige o token ficar acessível ao JS (em `localStorage` ou memória). Vulnerável a XSS. Para SPAs, normalmente se combina com tokens curtos (15min) + refresh token em cookie HttpOnly.
- **OpenID Connect + cookie de sessão** (IdentityServer, Duende, OpenIddict, Azure AD): adiciona um Authorization Server externo. Faz sentido quando você tem mais de um app consumindo a mesma identidade ou precisa de SSO.
- **ASP.NET Core Identity** (`AddIdentity`): traz UserManager, RoleManager, fluxos de e-mail/confirmação/2FA prontos. Vale a pena quando o sistema precisa de gestão completa de usuários — para um sistema pequeno com tabela `Docente` própria, é overkill.
- **Session em memória** (`AddSession`): cookie só tem um ID opaco e o estado da sessão fica server-side. Resolve o problema de tamanho do cookie (claims pesadas), mas exige cache distribuído em multi-instância e perde dados num restart.

---

## 3. Antiforgery — Double-Submit Cookie (proteção CSRF)

### (A) Implementação

Adicione em `Program.cs` (perto da `AddCors`):

```csharp
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name        = "XSRF-TOKEN";
    options.Cookie.HttpOnly    = false;          // CRÍTICO: JS PRECISA ler este cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite    = SameSiteMode.Lax;
    options.HeaderName         = "X-XSRF-TOKEN";
});
```

Ative o filtro automático em `AddControllersWithViews`:

```csharp
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());

    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        _ => "Este campo é obrigatório.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (_, _) => "O valor informado é inválido.");
});
```

Crie `GestãoDeTurmas/Middlewares/AntiforgeryCookieMiddleware.cs`:

```csharp
using Microsoft.AspNetCore.Antiforgery;

namespace GestãoDeTurmas.Middlewares;

public class AntiforgeryCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryCookieMiddleware(RequestDelegate next, IAntiforgery antiforgery)
    {
        _next = next;
        _antiforgery = antiforgery;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tokens = _antiforgery.GetAndStoreTokens(context);
        context.Response.Headers["X-XSRF-TOKEN"] = tokens.RequestToken!;
        await _next(context);
    }
}
```

### (B) Conceitos Detalhados

#### O que é CSRF (Cross-Site Request Forgery)

Cenário do ataque:

1. Usuário está logado em `https://app.banco.com` (cookie de sessão ativo).
2. Em outra aba, abre `https://evil.com`.
3. `evil.com` contém:
   ```html
   <form action="https://app.banco.com/api/transferir" method="POST">
     <input name="destino" value="conta-do-atacante">
     <input name="valor" value="10000">
   </form>
   <script>document.forms[0].submit()</script>
   ```
4. O navegador faz o `POST` e **anexa automaticamente** o cookie de `app.banco.com`. O servidor vê uma requisição autenticada legítima e transfere o dinheiro.

O atacante **não precisa ler** a resposta (a SOP impede). Ele só precisa que a requisição seja processada. CSRF se aproveita exatamente desse envio automático do cookie pelo navegador.

#### Por que `SameSite=Lax` sozinho não basta

`SameSite=Lax` bloqueia o envio do cookie em requisições cross-site iniciadas via `fetch`, `XHR`, `img`, `iframe` e `form POST`. Isso fecha a maioria dos vetores CSRF clássicos, mas:

- Subdomínios são considerados same-site (`evil.empresa.com` ataca `app.empresa.com`).
- Implementações em browsers antigos (incluindo navegadores corporativos engessados) podem não respeitar `Lax`.
- Configurações específicas (uso de `SameSite=None` por necessidade legítima) reabrem o vetor.

Por isso, `SameSite=Lax` é **defense-in-depth**, e a defesa principal continua sendo o token anti-CSRF.

#### Padrões de proteção CSRF (e qual é o adotado aqui)

1. **Synchronizer Token Pattern**: servidor mantém o token em sessão server-side, embute em formulários (`<input type="hidden">`) e valida no POST. Funciona bem em apps MVC tradicionais, mas exige estado no servidor.
2. **Double-Submit Cookie** *(o adotado)*: servidor grava o token tanto em um cookie acessível ao JS (`XSRF-TOKEN`) quanto valida que o cliente o reenvia em um header (`X-XSRF-TOKEN`). O cliente lê o cookie via `document.cookie` e copia para o header. **Stateless** no servidor.
3. **Custom Header Check**: simplesmente exigir um header customizado (`X-Requested-With: XMLHttpRequest`). Funciona porque adicionar header customizado em cross-origin dispara CORS preflight. Frágil — se o servidor expor CORS de modo permissivo, cai.
4. **Origin/Referer Validation**: rejeitar requisições cuja `Origin` ou `Referer` não bata com o esperado. Defense-in-depth — alguns clientes legítimos não enviam esses headers.

A implementação `IAntiforgery` do .NET combina (2) com uma camada criptográfica: os dois tokens (cookie e request) **não são iguais**, são tokens correlacionados que precisam validar juntos contra a mesma chave da Data Protection API. Isso impede que um atacante que conheça (ou adivinhe) o formato do token o forje a partir de informações públicas.

#### Por que `HttpOnly = false` no cookie `XSRF-TOKEN`

Inversão deliberada: o cookie de autenticação é `HttpOnly` (esconde do JS), mas o cookie CSRF **precisa** ser lido pelo JS para ser copiado ao header. Não há vulnerabilidade aqui — o token CSRF não dá acesso a nada por si só; ele só faz sentido em combinação com o cookie de auth. Se o atacante consegue rodar JS na sua origem (XSS), ele já tem acesso a coisas piores, e o token CSRF é o menor dos problemas.

O que dá segurança é: **um site externo (`evil.com`) não consegue ler o cookie `XSRF-TOKEN` da sua origem** por causa da Same-Origin Policy de cookies. Portanto, mesmo enviando o cookie de auth automaticamente, ele não consegue forjar o header.

#### Por que o nome `XSRF-TOKEN` / `X-XSRF-TOKEN`

Convenção criada pelo AngularJS e adotada por Angular moderno: o `HttpClient` do Angular **automaticamente** detecta o cookie chamado `XSRF-TOKEN` e o copia para o header `X-XSRF-TOKEN` em toda requisição mutante. Mantendo esses nomes, você não precisa escrever nenhum interceptor manual — o Angular faz sozinho.

Se quiser outros nomes (ex.: `__RequestVerificationToken`, padrão antigo do .NET), você consegue, mas terá que configurar o `withXsrfConfiguration({ cookieName, headerName })` no Angular e o `options.HeaderName` no .NET batendo. **Manter a convenção é menos código e menos chance de erro.**

#### `AutoValidateAntiforgeryTokenAttribute` vs `ValidateAntiForgeryToken`

- **`[ValidateAntiForgeryToken]`** *(decorando endpoint)*: valida sempre. Verboso, e fácil esquecer em algum endpoint novo.
- **`[AutoValidateAntiforgeryToken]`** *(filtro global ou decorando classe)*: valida em todos os métodos com verbo "não seguro" (`POST`, `PUT`, `PATCH`, `DELETE`) e pula `GET`, `HEAD`, `OPTIONS`, `TRACE`. **Esse é o usado neste guia.**
- **`[IgnoreAntiforgeryToken]`** *(decorando endpoint)*: opt-out. Necessário em endpoints como `/api/auth/login` (o usuário ainda não tem cookie XSRF — primeiro request) e potencialmente em webhooks vindos de terceiros.

#### `GetAndStoreTokens` — o que ele faz exatamente

O método:

1. Procura um cookie `XSRF-TOKEN` existente.
2. Se existir, deriva um novo `requestToken` correlacionado (não usa o mesmo valor — usa um nonce).
3. Se não existir, gera ambos.
4. Grava o cookie token (`Set-Cookie: XSRF-TOKEN=...`).
5. Retorna o `requestToken` em `tokens.RequestToken`.

O `AntiforgeryCookieMiddleware` chama isso em **toda** requisição (após auth) para garantir que sempre que o usuário ganhar uma identidade (após login), o token seja regenerado vinculado a essa identidade. Importante: tokens emitidos para um usuário anônimo **não são válidos** após login, e vice-versa — o token carrega o `userId` de quem o emitiu. O middleware aqui resolve isso emitindo um token novo a cada request.

**Custo**: emitir tokens é barato (criptografia simétrica), mas se a performance for crítica, dá para emitir apenas em GETs específicos (rota `/api/antiforgery/token`) e fazer o Angular buscar uma vez por sessão.

#### Como o Angular se comunica

```typescript
// Angular 18+
provideHttpClient(
  withXsrfConfiguration({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' })
)
```

Com isso, em cada `POST`/`PUT`/`PATCH`/`DELETE`:

1. O `HttpXsrfInterceptor` lê `document.cookie['XSRF-TOKEN']`.
2. Adiciona `X-XSRF-TOKEN: <valor>` na requisição.
3. O servidor valida e processa.

Por padrão, o Angular **só** envia o header para URLs **relativas** ou para a mesma origem. Para envio cross-origin (caso clássico de SPA + API separadas em dev), você precisa garantir que o request inclua o cookie (`withCredentials: true`) — caso contrário o navegador nem envia o cookie, e o interceptor do Angular tampouco encontra o cookie para ler.

#### Alternativas

- **Tokens em meta tag + header** (padrão Razor Pages clássico): o token é renderizado dentro do HTML server-rendered. SPAs não têm essa fase, então o cookie é mais natural.
- **Validação de `Origin` apenas**: válido como defense-in-depth, mas insuficiente sozinho. Combine.
- **`SameSite=Strict` em cookies + sem antiforgery**: defendível em sistemas pequenos onde nunca há navegação cross-site legítima, mas você perde flexibilidade (e ainda há subdomínio attacks).
- **CAPTCHA em ações críticas**: defesa adicional, não substituta. Útil em transferências financeiras, mas friction-heavy.

---

## 4. Pipeline final do `Program.cs`

### (A) Implementação

```csharp
var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHsts();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("PermitirAngular");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GestãoDeTurmas.Middlewares.AntiforgeryCookieMiddleware>();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PaginaInicial}/{action=Index}/{id?}");

app.Run();
```

### (B) Conceitos Detalhados

#### O modelo de pipeline (Russian Dolls)

Cada chamada `app.UseXxx` registra um **middleware**: uma função `(HttpContext, RequestDelegate next) → Task`. Em runtime, o ASP.NET compõe esses middlewares em uma cadeia:

```
Request →  ExceptionHandler →  HSTS  → HttpsRedirection → Routing → CORS → Auth(N) → Auth(Z) → Antiforgery → Endpoint
                                                                                                                  ↓
Response ← ExceptionHandler ← HSTS  ← HttpsRedirection ← Routing ← CORS ← Auth(N) ← Auth(Z) ← Antiforgery ← Endpoint
```

Cada middleware decide:
- Executar lógica antes do `next()` (no caminho do request).
- Chamar `next()` para passar adiante.
- Executar lógica depois do `next()` (no caminho da response).
- Ou interromper a cadeia (short-circuit), respondendo direto — usado por `UseAuthorization` quando o request não está autorizado.

**A ordem de registro é a ordem de execução.** Trocar de posição pode quebrar tudo silenciosamente.

#### Por que cada middleware fica nessa posição

| Middleware | Posição | Justificativa |
|---|---|---|
| `UseExceptionHandler` | Topo | Precisa envolver todos os outros para capturar qualquer exceção lançada adiante. |
| `UseHsts` | Cedo | Adiciona o header `Strict-Transport-Security`, força clientes a usar HTTPS futuro. |
| `UseHttpsRedirection` | Cedo | Redireciona requisições HTTP para HTTPS antes de tocar em qualquer outra coisa. |
| `UseRouting` | Antes de CORS/Auth | Resolve o endpoint (`HttpContext.GetEndpoint()`), permitindo que CORS e Auth saibam qual policy/política aplicar para a rota específica. |
| `UseCors` | Depois de Routing, antes de Auth | Precisa estar antes de Auth: o preflight `OPTIONS` é anônimo e não deve passar por validação de identidade. Mas precisa depois de Routing para conhecer o endpoint. |
| `UseAuthentication` | Depois de CORS | Lê o cookie e popula `HttpContext.User`. **Sem isso, o User é sempre anônimo** (era o bug no código atual). |
| `UseAuthorization` | Depois de Authentication | Avalia `[Authorize]` contra `HttpContext.User`. Se reprovar, short-circuita com 401/403. |
| `AntiforgeryCookieMiddleware` | Depois de Authorization | O token é emitido **com a identidade já resolvida**, garantindo correlação cookie↔request token. |
| `MapControllers` | Final | Mapeia o endpoint efetivo (executa o método do controller). |

#### Por que `UseRouting` vem antes de `UseCors`

Antes do .NET 6, a ordem comum era `UseCors` antes de `UseRouting`. Hoje, com **Endpoint Routing**, o `UseRouting` resolve o endpoint **mas não o executa**. Isso permite que middlewares posteriores (CORS, Auth) examinem metadados do endpoint (`[EnableCors("policy")]`, `[Authorize]`) e tomem decisões específicas. Por exemplo, dá pra ter endpoints diferentes com policies CORS diferentes:

```csharp
[EnableCors("PermitirAngular")]
public class AlunosController { ... }

[EnableCors("PermitirParceiros")]
public class WebhookController { ... }
```

#### O bug no código atual

`Program.cs:93-97` faz:

```csharp
app.UseRouting();
app.UseCors("PermitirAngular");
app.UseAuthorization();
// FALTA: app.UseAuthentication();
```

Sem `UseAuthentication`, o `User` é sempre anônimo, então o `UseAuthorization` rejeita tudo com 401, **mesmo após login bem-sucedido**. É um dos bugs mais comuns em apps ASP.NET — silencioso porque o `SignInAsync` funciona (cookie é gravado), mas ninguém lê o cookie de volta.

#### Alternativas

- **Minimal APIs** (`app.MapGet`, `app.MapPost`): mais enxuto, mas perde o roteamento por convenção e os filtros automáticos. Para um sistema com vários controllers já estabelecidos, vale manter MVC.
- **Endpoint metadata customizado** (`RequireAuthorization`, `RequireCors`): permite configurar policies por endpoint diretamente, sem atributos. Útil em minimal APIs.

---

## 5. `AuthController` — `login`, `logout`, `me`

### (A) Implementação

Primeiro, adicione campos em `Common/Domains/Docente.cs`:

```csharp
public class Docente : IEntidadeInativavel
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string? Email { get; set; }
    public DateOnly DataNascimento { get; set; }
    public int? DisciplinaId { get; set; }
    public Disciplina? Disciplina { get; set; }
    public bool Ativo { get; set; } = true;

    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Docente";
}
```

Gere a migration e atualize o banco:

```powershell
dotnet ef migrations add AdicionarAuth -p Repository -s GestãoDeTurmas
dotnet ef database update -p Repository -s GestãoDeTurmas
```

Instale o BCrypt:

```powershell
dotnet add GestãoDeTurmas package BCrypt.Net-Next
```

Adicione `ObterPorEmailAsync` em `IDocenteService` e `DocenteService` (consultando `_docenteRepository`).

Substitua `GestãoDeTurmas/Controllers/AuthController.cs`:

```csharp
using System.Security.Claims;
using API.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IDocenteService _docenteService;

    public AuthController(IDocenteService docenteService)
    {
        _docenteService = docenteService;
    }

    public record LoginInput(string Email, string Senha);
    public record UsuarioPublicoOutput(int Id, string Nome, string Email, string Role);

    [HttpPost("login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Login([FromBody] LoginInput input)
    {
        var docente = await _docenteService.ObterPorEmailAsync(input.Email);
        if (docente is null || !docente.Ativo)
            return Unauthorized(new { mensagem = "Credenciais inválidas." });

        if (!BCrypt.Net.BCrypt.Verify(input.Senha, docente.SenhaHash))
            return Unauthorized(new { mensagem = "Credenciais inválidas." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, docente.Id.ToString()),
            new(ClaimTypes.Name, docente.Nome),
            new(ClaimTypes.Email, docente.Email ?? string.Empty),
            new(ClaimTypes.Role, docente.Role)
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
            });

        return Ok(new UsuarioPublicoOutput(docente.Id, docente.Nome, docente.Email!, docente.Role));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var id    = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var nome  = User.FindFirstValue(ClaimTypes.Name);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role  = User.FindFirstValue(ClaimTypes.Role);

        if (id is null) return Unauthorized();

        return Ok(new UsuarioPublicoOutput(int.Parse(id), nome ?? "", email ?? "", role ?? ""));
    }
}
```

### (B) Conceitos Detalhados

#### `Claim`, `ClaimsIdentity` e `ClaimsPrincipal`

Esses três tipos são a forma como o .NET representa "quem é o usuário":

- **`Claim`** = um par chave-valor representando uma asserção sobre o usuário. Ex.: `("email", "joao@empresa.com")` significa "este usuário afirma ter este e-mail". A chave geralmente segue uma URI do padrão SAML/JWT (`http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress`); a classe `ClaimTypes` traz constantes para os mais comuns.
- **`ClaimsIdentity`** = uma coleção de claims **vindas da mesma fonte** (no nosso caso, do cookie). Ela carrega também o nome do scheme de autenticação (`CookieAuthenticationDefaults.AuthenticationScheme`), que é o que diz "essas claims vieram de quem".
- **`ClaimsPrincipal`** = um *usuário*, que pode ter **uma ou mais** `ClaimsIdentity`. Por isso `User.Claims` agrega claims de todas as identities. Útil em cenários com múltiplos providers (ex.: usuário logado no Google + claims locais).

**Por que `ClaimTypes.NameIdentifier` (e não `"id"`)?** Convenção. `User.FindFirstValue(ClaimTypes.NameIdentifier)` é o padrão idiomático; mantendo claims com URIs padrão, qualquer outra parte do framework (Identity, OpenID) conversa naturalmente.

#### `HttpContext.SignInAsync` — o que ele faz

Internamente:

1. Resolve o `IAuthenticationService` do DI container.
2. Encontra o handler do scheme passado (`CookieAuthenticationDefaults.AuthenticationScheme`).
3. Chama `handler.SignInAsync(principal, properties)` — no `CookieAuthenticationHandler`, isso serializa o principal num `AuthenticationTicket`, cifra com `IDataProtector` (chave do *Data Protection*), e escreve o `Set-Cookie`.

`AuthenticationProperties`:
- `IsPersistent = true` → o cookie tem `Expires` e sobrevive a fechamento do navegador. `false` → cookie de sessão (some quando fecha o navegador).
- `ExpiresUtc` → sobrescreve o `ExpireTimeSpan` global para este sign-in específico.
- `AllowRefresh` → controla se a sliding expiration se aplica a esse ticket.

#### `[IgnoreAntiforgeryToken]` no `login`

O endpoint `/login` é o primeiro contato. O usuário ainda **não tem** cookie `XSRF-TOKEN` válido vinculado à sua identidade (no máximo, tem um vinculado ao "anônimo"). Exigir antiforgery aqui criaria um problema do ovo-e-galinha:

- Para validar antiforgery, o cliente precisa do cookie XSRF.
- Para emitir o cookie XSRF vinculado ao usuário, o usuário precisa fazer login.

Solução: o login dispensa antiforgery (`[IgnoreAntiforgeryToken]`) e a primeira coisa que ele faz é validar credenciais — o que substitui a defesa CSRF, já que um atacante remoto não conhece a senha.

**Mitigação adicional**: rate-limiting agressivo em `/login` (5 tentativas/min/IP) bloqueia ataques de força bruta que se aproveitariam dessa exceção.

#### BCrypt — por que não SHA256 ou MD5

Senhas **nunca** devem ser armazenadas em hash rápido (SHA256, MD5, SHA1). Hashes rápidos são o que torna ataque por força bruta viável: uma GPU moderna calcula bilhões de SHA256 por segundo. Se um banco vaza, o atacante quebra senhas comuns em segundos.

BCrypt, Argon2id e Scrypt são **hashes propositalmente lentos** (work factor configurável) e usam **salt automático** (cada hash tem salt único, embutido no resultado). Resultado: força bruta vira inviável computacionalmente. BCrypt padrão (work factor 11) leva ~100ms por hash — imperceptível no login, devastador para o atacante.

`BCrypt.Net.BCrypt.HashPassword(senha)` gera algo como `$2a$11$saltbase64.hashbase64`. `Verify(senha, hash)` extrai o salt embutido, recalcula e compara em tempo constante (proteção contra timing attacks).

**Alternativa**: Argon2id (mais moderno, vencedor da Password Hashing Competition de 2015) — disponível via pacote `Konscious.Security.Cryptography.Argon2`. Mais resistente a ataques com hardware especializado (ASIC), mas BCrypt ainda é considerado seguro para uso corporativo.

#### `SignOutAsync` — como o logout funciona

Não há "destruir cookie" no protocolo HTTP — o servidor não consegue forçar o cliente a esquecer. O que o `SignOutAsync` faz é enviar:

```
Set-Cookie: GestaoTurmas.Auth=; Expires=Thu, 01-Jan-1970 00:00:00 GMT; Path=/; Secure; HttpOnly
```

O navegador, ao receber um cookie com `Expires` no passado, **descarta** o cookie da sua store. Importante: como o `Set-Cookie` substituível precisa bater no `Name`, `Path` e `Domain`, o handler usa exatamente os mesmos atributos do sign-in original.

**Observação de segurança**: o `SignOutAsync` não invalida o ticket no servidor (o cookie é stateless por design). Se um atacante já roubou o cookie antes do logout, ele continua válido até o `ExpiresUtc`. Mitigação: implementar uma denylist server-side de tickets revogados, ou usar tickets curtos (15min) + renovação via endpoint `/api/auth/refresh`.

#### `[Authorize]` em `/logout` e `/me`

Esses endpoints precisam de usuário autenticado para fazer sentido. O `[Authorize]` resolve isso. Note que o `[AutoValidateAntiforgeryToken]` global continua valendo no `/logout` (é um POST mutante) — boa prática, mesmo sendo "só" um logout.

#### Alternativas

- **ASP.NET Core Identity completo**: traz `UserManager`, fluxos de e-mail/2FA, lockout após tentativas falhas. Para sistemas grandes, evita reinventar a roda. Para este sistema (com tabela `Docente` própria), é overkill.
- **OpenID Connect (Azure AD, Auth0, Keycloak)**: ideal se você precisa de SSO entre múltiplos apps, integração com AD corporativo, ou MFA pronto.
- **Magic links** (login sem senha, via e-mail): elimina senhas. Bom para apps internos onde o e-mail corporativo já é seguro.

---

## 6. Autorização nos controllers (Roles e Policies)

### (A) Implementação

Decorando o `AlunosController`:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlunosController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObterTodosOsAlunos(...) { ... }

    [HttpPost]
    [Authorize(Roles = "Admin,Coordenador")]
    public async Task<IActionResult> AdicionarAluno([FromBody] AlunoInputViewModel novoAluno) { ... }

    [HttpPatch("{id}/inativar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> InativarAluno(int id) { ... }
}
```

Para políticas mais ricas, em `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GestorAcademico", p => p.RequireRole("Admin", "Coordenador"));
    options.AddPolicy("SomenteDocente", p => p.RequireRole("Docente"));
});
```

E no controller:

```csharp
[Authorize(Policy = "GestorAcademico")]
public async Task<IActionResult> AdicionarAluno(...) { ... }
```

### (B) Conceitos Detalhados

#### `[Authorize]` vs `[AllowAnonymous]`

- **`[Authorize]`** no controller torna **todos** os endpoints daquele controller protegidos. Aplicar individualmente em métodos é equivalente, mas mais verboso.
- **`[AllowAnonymous]`** em um endpoint específico cria uma "ilha" pública dentro de um controller protegido. Útil para endpoints como `/api/health` ou `/api/auth/login`.
- A precedência é simples: `[AllowAnonymous]` **sempre** vence `[Authorize]` (não há override de policy — é um curto-circuito explícito).

#### Como o `AuthorizationMiddleware` funciona internamente

1. O `UseRouting` resolveu o endpoint e anexou metadata (incluindo os atributos `[Authorize]`/`[AllowAnonymous]` do método e da classe).
2. O `UseAuthorization` lê esses metadados, monta uma `AuthorizationPolicy` (combinando todos os `[Authorize]` aplicáveis) e chama `IAuthorizationService.AuthorizeAsync(user, policy)`.
3. Cada requisito da policy (`RequireAuthenticatedUser`, `RequireRole`, `RequireClaim`) é avaliado por um `AuthorizationHandler<TRequirement>`.
4. Se algum handler falhar, o middleware short-circuita:
   - Usuário não autenticado → 401 (via `OnRedirectToLogin`, configurado para devolver 401 puro).
   - Usuário autenticado mas sem a role/claim necessária → 403 (via `OnRedirectToAccessDenied`).

#### Roles vs Claims vs Policies

- **Roles**: agrupamento simples. Bom para o caso "usuário pode ou não pode acessar esta tela". `[Authorize(Roles = "Admin")]`.
- **Claims arbitrárias**: granularidade maior. `[Authorize(Policy = "MaiorDeIdade")]` onde a policy é `p.RequireClaim("idade", ageGroups => int.Parse(ageGroups.First()) >= 18)`.
- **Policies**: forma idiomática de empacotar requisitos. Permite reuso (`"GestorAcademico"` aplicado em vários controllers, mudança centralizada).
- **Resource-based authorization** (`IAuthorizationService.AuthorizeAsync(user, recurso, policy)`): para perguntas como "este usuário pode editar **este** aluno em particular?". Ex.: docente só edita alunos da sua disciplina.

**Recomendação para este sistema**: usar Roles para grandes categorias (`Admin`, `Coordenador`, `Docente`) e Policies para regras transversais. Reservar Resource-based para endpoints com regras de domínio.

#### 401 vs 403

- **401 Unauthorized**: "Não sei quem você é, faça login." → o cliente deve mostrar tela de login.
- **403 Forbidden**: "Sei quem você é, mas você não pode fazer isso." → o cliente deve mostrar "Acesso negado".

Confundir os dois prejudica UX e segurança (devolver 403 quando deveria ser 401 vaza a existência do recurso para anônimos). O .NET acerta isso automaticamente quando os eventos `OnRedirectToLogin`/`OnRedirectToAccessDenied` estão configurados.

#### Alternativas

- **ABAC (Attribute-Based Access Control)**: cada decisão considera atributos do usuário, do recurso, da ação e do contexto. Implementável com policies + resource-based handlers. Overkill para sistemas pequenos.
- **PBAC (Policy-Based Access Control)** com um serviço externo (Open Policy Agent, Cerbos): centraliza regras de autorização fora do código. Vale a pena quando regras mudam frequentemente sem deploy.
- **Permission claims granulares** (`"alunos:create"`, `"alunos:delete"`): mais flexível que roles, mas exige UI de admin para gerenciar permissões.

---

## 7. Ajuste no `GlobalExceptionHandler`

### (A) Implementação

Substitua `GestãoDeTurmas/Middlewares/GlobalExceptionHandler.cs`:

```csharp
using Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        _logger.LogError(ex, "Erro inesperado: {Mensagem}", ex.Message);

        var (status, titulo) = ex switch
        {
            EntidadeNaoEncontradaException => (StatusCodes.Status404NotFound,         "Recurso não encontrado"),
            RegraDeNegocioException        => (StatusCodes.Status400BadRequest,       "Regra de negócio violada"),
            UnauthorizedAccessException    => (StatusCodes.Status401Unauthorized,     "Não autenticado"),
            _                              => (StatusCodes.Status500InternalServerError, "Erro inesperado")
        };

        ctx.Response.StatusCode  = status;
        ctx.Response.ContentType = "application/problem+json";

        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title  = titulo,
            Detail = ex.Message
        }, cancellationToken: ct);

        return true;
    }
}
```

### (B) Conceitos Detalhados

#### Por que não fazer `Redirect`

O código original (`Middlewares/GlobalExceptionHandler.cs:22-27`) faz `Response.Redirect("/Home/Error")`. Isso funciona para apps MVC server-rendered, mas para uma SPA:

- O `HttpClient` do Angular segue o 302 (`followRedirects` padrão), recebe HTML em vez do JSON esperado, e a desserialização falha.
- O usuário vê "Unexpected end of JSON input" no console e nada na tela.

A regra: APIs devolvem **status codes e JSON estruturado**. Redirects são uma decisão de UX que cabe ao cliente.

#### `ProblemDetails` (RFC 7807)

`ProblemDetails` é um formato padronizado para erros HTTP, definido na RFC 7807. Schema:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Recurso não encontrado",
  "status": 404,
  "detail": "Aluno com id 42 não foi encontrado.",
  "instance": "/api/alunos/42"
}
```

Vantagens:
- Padrão reconhecido por bibliotecas client (Angular, Refit, OpenAPI generators).
- Permite estender com campos customizados (`errors`, `traceId`).
- Documentável via Swagger.

O método `Results.Problem(...)` e a classe `Microsoft.AspNetCore.Mvc.ProblemDetails` já produzem esse formato com `Content-Type: application/problem+json`.

#### `IExceptionHandler` vs middleware customizado

O `IExceptionHandler` (introduzido no .NET 8) é injetado via DI e registrado via `AddExceptionHandler<T>` + `UseExceptionHandler()`. Vantagens sobre um middleware customizado clássico:

- DI nativo (logger, IOptions etc.).
- Ordem explícita: vários `IExceptionHandler` registrados rodam em sequência até um devolver `true`.
- Integra com `IProblemDetailsService` (que aplica `IProblemDetailsWriter` se você quiser customização avançada).

#### Alternativas

- **`ExceptionFilter`** (`[Filter]` decorando controllers ou global): roda dentro do pipeline MVC, depois do model binding. Captura menos coisa (middlewares anteriores erram fora dele). Útil para erros de domínio específicos por controller.
- **Middleware manual com `try/catch`**: total controle, mas exige reinventar tudo que o `UseExceptionHandler` já faz (rewriting response, evitar dupla escrita etc.).
- **Endpoint filters em minimal APIs**: equivalente a Action filters para minimal APIs.

---

## 8. Configuração esperada no Angular

Embora o foco deste documento seja o backend, vale registrar como o Angular precisa estar configurado para tudo funcionar.

### Setup do `HttpClient`

```typescript
// app.config.ts
import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withFetch, withXsrfConfiguration, withInterceptors } from '@angular/common/http';
import { credentialsInterceptor } from './interceptors/credentials.interceptor';
import { authErrorInterceptor } from './interceptors/auth-error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withFetch(),
      withXsrfConfiguration({
        cookieName: 'XSRF-TOKEN',
        headerName: 'X-XSRF-TOKEN'
      }),
      withInterceptors([
        credentialsInterceptor,
        authErrorInterceptor
      ])
    )
  ]
};
```

### Interceptor de credenciais

```typescript
// credentials.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) =>
  next(req.clone({ withCredentials: true }));
```

> **Por que precisa?** Sem `withCredentials: true`, o navegador descarta cookies em requisições cross-origin, e o `HttpXsrfInterceptor` do Angular fica sem cookie pra ler.

### Interceptor de erro 401

```typescript
// auth-error.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        router.navigate(['/login']);
      }
      return throwError(() => err);
    })
  );
};
```

### `AuthService` no Angular

```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly _user = signal<UsuarioPublico | null>(null);
  readonly user = this._user.asReadonly();

  login(email: string, senha: string) {
    return this.http.post<UsuarioPublico>('/api/auth/login', { email, senha })
      .pipe(tap(u => this._user.set(u)));
  }

  logout() {
    return this.http.post<void>('/api/auth/logout', {})
      .pipe(tap(() => this._user.set(null)));
  }

  // Chamado uma vez no bootstrap da app, antes do roteamento
  me() {
    return this.http.get<UsuarioPublico>('/api/auth/me')
      .pipe(
        tap(u => this._user.set(u)),
        catchError(() => { this._user.set(null); return of(null); })
      );
  }
}
```

---

## 9. Fluxo end-to-end

```
[Angular SPA]                                       [Backend .NET]
    │
    │ 1. GET /api/auth/me  (bootstrap inicial)
    │    Cookie: (nenhum ainda)
    │                                          ┌──► UseCors
    │                                          ├──► UseAuthentication (não acha cookie → User anônimo)
    │                                          ├──► UseAuthorization (rejeita → 401)
    │                                          └──► AntiforgeryCookieMiddleware grava XSRF-TOKEN
    │ ◄────  401 Unauthorized
    │        Set-Cookie: XSRF-TOKEN=...; Secure; SameSite=Lax
    │
    │  → Angular intercepta 401 e redireciona para /login
    │
    │ 2. POST /api/auth/login  { email, senha }
    │    Cookie: XSRF-TOKEN=...
    │    (esse endpoint não exige X-XSRF-TOKEN, [IgnoreAntiforgeryToken])
    │                                          ► AuthController.Login
    │                                          ► BCrypt.Verify
    │                                          ► SignInAsync
    │ ◄────  200 OK { id, nome, email, role }
    │        Set-Cookie: GestaoTurmas.Auth=<ticket cifrado>; HttpOnly; Secure; SameSite=Lax
    │        Set-Cookie: XSRF-TOKEN=<token vinculado ao usuário>; Secure; SameSite=Lax
    │
    │ 3. GET /api/auth/me
    │    Cookie: GestaoTurmas.Auth=...; XSRF-TOKEN=...
    │                                          ► UseAuthentication popula User
    │ ◄────  200 OK { id, nome, email, role }
    │
    │ 4. POST /api/alunos  { ... }   (request mutante)
    │    Cookie: GestaoTurmas.Auth=...; XSRF-TOKEN=...
    │    Header: X-XSRF-TOKEN: <valor do cookie>
    │                                          ► UseAuthentication popula User
    │                                          ► UseAuthorization aprova [Authorize(Roles="Admin")]
    │                                          ► AutoValidateAntiforgery valida cookie↔header
    │                                          ► AlunosController.AdicionarAluno
    │ ◄────  200 OK
    │
    │ 5. POST /api/auth/logout
    │ ◄────  204 No Content
    │        Set-Cookie: GestaoTurmas.Auth=; Expires=01-Jan-1970
```

---

## 10. Checklist de implementação

- [ ] **Domínio** — adicionar `SenhaHash` e `Role` em `Common/Domains/Docente.cs:3-13`.
- [ ] **Migration** — `dotnet ef migrations add AdicionarAuth -p Repository -s GestãoDeTurmas` + `database update`.
- [ ] **Pacote** — `dotnet add GestãoDeTurmas package BCrypt.Net-Next`.
- [ ] **Service** — adicionar `Task<Docente?> ObterPorEmailAsync(string email)` em `IDocenteService` / `DocenteService` / `IDocenteRepository` / `DocenteRepository`.
- [ ] **Program.cs** — atualizar `AddCors` (seção 1), `AddCookie` (seção 2), adicionar `AddAntiforgery` (seção 3), corrigir pipeline (seção 4) incluindo `UseAuthentication`, `MapControllers` e o `AntiforgeryCookieMiddleware`.
- [ ] **Middleware** — criar `GestãoDeTurmas/Middlewares/AntiforgeryCookieMiddleware.cs`.
- [ ] **AuthController** — substituir `GestãoDeTurmas/Controllers/AuthController.cs` pelo código da seção 5.
- [ ] **Authorize** — decorar `AlunosController`, `DocentesController`, `GerenciarTurmaController` etc. com `[Authorize]` e `[Authorize(Roles=...)]` conforme regras de negócio.
- [ ] **GlobalExceptionHandler** — substituir `Redirect` por `ProblemDetails` (seção 7).
- [ ] **Angular** — `withXsrfConfiguration`, interceptor `withCredentials: true`, interceptor de 401.
- [ ] **Smoke test** — `/api/auth/me` antes do login devolve 401 com cookie XSRF; `/api/auth/login` devolve 200 e seta os dois cookies; `/api/alunos` POST sem header X-XSRF-TOKEN devolve 400; com header e role correta, processa.

---

## Referências

- RFC 6265 — HTTP State Management Mechanism (cookies)
- RFC 7807 — Problem Details for HTTP APIs
- [Fetch Standard — CORS](https://fetch.spec.whatwg.org/#http-cors-protocol)
- [OWASP CSRF Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [Microsoft Docs — ASP.NET Core Authentication](https://learn.microsoft.com/aspnet/core/security/authentication/)
- [Microsoft Docs — Antiforgery in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery)
- [Microsoft Docs — Enable Cross-Origin Requests (CORS)](https://learn.microsoft.com/aspnet/core/security/cors)
