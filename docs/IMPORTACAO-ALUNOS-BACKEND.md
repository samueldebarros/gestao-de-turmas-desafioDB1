# Importação de Alunos em Massa (.NET + EF Core) — endpoint transacional `POST /api/alunos/importar`

> **Escopo:** o lado **origem/servidor** da importação de alunos por planilha — o contrato de request/response, a **validação server-side** por linha, a **persistência atômica** (tudo-ou-nada) e o endpoint no `AlunosController`. Fecha o handoff que o front deixou em aberto: **C1** (não há `POST /api/alunos/importar`) e **C2** (`POST /api/alunos` devolve `Ok()` sem corpo, sem ID).
>
> **Fonte da verdade:** **este documento (back) é a verdade absoluta do que se persiste.** O front (`COMPONENTE-FILE-UPLOAD.md`, §5) faz **parse + preview** client-side, mas quem **valida de verdade, gera matrícula e grava** é este lado. Onde a validação do front divergir daqui, vence daqui. Cada seção fecha com **(C) Comunicação com o front**, mapeando para `[FRONT §5.N]`.
>
> **Autossuficiência:** este guia é **completo**. Todo método citado aparece com o **corpo inteiro** e uma explicação — não há "esboço", placeholder nem "estenda o método X". Cada bloco de código está marcado como **[JÁ EXISTE]** (está no repositório hoje), **[NOVO]** (você vai criar) ou **[ALTERAR]** (você vai modificar um arquivo existente). Ao final de cada mudança, o **arquivo/caminho exato** é indicado.
>
> **Código-fonte:** vive **neste mesmo repositório** (`DesafioTecnico1-Fundamentos/`). A arquitetura é em camadas, e cada tipo mora num projeto diferente — os caminhos abaixo são **links reais** relativos a `docs/`. Só os links do **front** (`../src/app/…`) apontam para o outro repositório e continuam textuais.
>
> **Ambiente:** .NET 10, EF Core 10 sobre SQL Server (`GestaoEscolarContext`). O **host que roda a API é o projeto `GestãoDeTurmas`** (`https://localhost:7048`), que hospeda os controllers **e** as telas MVC. O projeto `API` **não** é o host — é uma biblioteca de _services_ + _DTOs_. Autenticação é **JWT lido de cookie** (`access_token`); papéis são strings livres em `Usuario.Role` (o seed cria `Admin`).
>
> ✅ **Estado verificado, não inferido:** tudo abaixo foi conferido contra o código real em 2026-07-20. Nomes, projetos, tipos, assinaturas e caminhos refletem o que está no repositório.

## Camadas e direção de dependência (importa para onde cada tipo mora)

```
Common  ←  Repository  ←  API  ←  GestãoDeTurmas (host: controllers + MVC)
(domínio,     (EF Core,     (services,   (recebe ViewModels, mapeia p/ DTO,
 enums,        repos,        DTOs)        try/catch, devolve HTTP)
 utils,        context)
 exceptions)
```

A seta `A ← B` = "B referencia A". A regra que decide **onde criar cada tipo novo**:

- Um tipo só pode referenciar tipos de camadas **à sua esquerda**. `Common` não enxerga `API`; `Repository` não enxerga `API`.
- Por isso a exceção `ImportacaoInvalidaException` (que carrega `LinhaErroDTO`, um DTO de `API`) **tem de morar em `API`**, não em `Common` — ver §3. A primeira versão deste doc errava ao mandar criá-la "no padrão de `Common/Exceptions`": isso **não compila**.

## Correções e decisões consolidadas (vs. a versão inferida)

| Tema | Versão inferida (errada) | Decisão consolidada (verificada) |
| --- | --- | --- |
| Projeto dos DTOs | `GestãoDeTurmas/DTOs/Alunos/` | [`API/DTOs/AlunoDTOs/`](../API/DTOs/AlunoDTOs/AlunoInputDTO.cs) — namespace `API.DTOs.AlunoDTOs` |
| Projeto do Service | `GestãoDeTurmas/Services/` | [`API/Service/`](../API/Service/AlunoService.cs) — namespace `API.Service` (pasta no singular) |
| DTO de entrada | `AlunoRequest` (novo) | [`AlunoInputDTO`](../API/DTOs/AlunoDTOs/AlunoInputDTO.cs) já existe; o controller unitário recebe o **ViewModel** [`AlunoInputViewModel`](../GestãoDeTurmas/Models/Aluno/AlunoInputViewModel.cs) e mapeia com [`.ToDTO()`](../GestãoDeTurmas/Mappers/AlunoMapperExtensions.cs). **O import vincula `List<AlunoInputDTO>` direto (decisão fechada, ver §1).** |
| Rota | `POST /alunos/importar` | `POST /api/alunos/importar` — `[Route("api/[controller]")]` |
| `sexo` no DTO | `int` | é `SexoEnum` (`Masculino=1, Feminino=2, Outro=3`), gravado **como string** no banco |
| `dataNascimento` no DTO | `string "yyyy-MM-dd"` | é `DateOnly` (o binder converte; parse manual desnecessário) |
| Geração de matrícula | sequencial `2026-0101` | [`GerarMatriculaUnicaAsync`](../API/Service/AlunoService.cs): `yyyyMM` + 4 chars hex maiúsculos de um GUID (ex.: `202607A3F2`), privado no service, com retry (5x) contra o banco. **No lote é estendido para evitar colisão intra-lote — ver §3.** |
| Validação de CPF | `CpfValido`/`SomenteDigitos` (novos) | [`Common/Utils/ValidacaoCpf`](../Common/Utils/ValidacaoCpf.cs): `IsCpfValido`, `Limpar`, `ValidarEProcessarCpfAsync` (já existem) |
| Acesso a dados | service usa `_context` + `BeginTransaction` | service **só** conhece [`IAlunoRepository`](../Repository/Repositories/AlunoRepository/IAlunoRepository.cs); `_context`/`_dbSet` são `protected` no [`BaseRepository`](../Repository/Repositories/Base/BaseRepository.cs). **`AddRange`/`SaveChanges`/consultas em lote são expostos por novos métodos de repositório — ver §2 e §3.** |
| Exceção → 422 | conversão automática por `ProblemDetails` | o [`GlobalExceptionHandler`](../GestãoDeTurmas/Middlewares/GlobalExceptionHandler.cs) faz **`Response.Redirect`** (MVC). Os controllers de API tratam com `try/catch` manual. **O 422 sai explícito do controller — ver §4.** |
| Dedupe | só CPF | **Email também tem índice único** ([`AlunoConfiguration`](../Repository/Mappings/AlunoConfiguration.cs)) → deduplicar CPF **e** email, intra-lote e inter-lote |
| `CreatedAtAction(nameof(ObterPorId))` | assume que existe | **não há** GET-by-id no `AlunosController`. **Decisão: usar `Created($"/api/alunos/{id}", dto)` — ver §4 e §5**, sem criar action nova |
| Roles da importação | `Admin,Coordenador` "espelhando" | a **classe** é `[Authorize(Roles="Admin,Coordenador,Docente")]`. **Decisão: restringir o import a `Admin,Coordenador`** (atributo no método). Isso é **interseção** com o da classe (AND), não "override" — ver §4 |
| Exceção de importação | `ImportacaoInvalidaException` em `Common` | **não pode morar em `Common`** (carrega `LinhaErroDTO` de `API`). **Decisão: criar em `API/Exceptions/` — ver §3** |
| `TotalCriado(s)` | plural no doc | o record real é **singular** (`TotalCriado`); JSON sai `totalCriado`. Alinhado neste doc |

## Índice

- [§0 — Visão geral e decisão de arquitetura](#0--visão-geral-e-decisão-de-arquitetura)
- [Estado atual (verificado no código)](#estado-atual-verificado-no-código)
- [§1 — Contrato: request e response da importação](#1--contrato-request-e-response-da-importação)
- [§2 — Validação server-side por linha](#2--validação-server-side-por-linha)
- [§3 — Persistência atômica (tudo-ou-nada) e camada de repositório](#3--persistência-atômica-tudo-ou-nada-e-camada-de-repositório)
- [§4 — Endpoint `POST /api/alunos/importar`](#4--endpoint-post-apialunosimportar)
- [§5 — `POST /api/alunos` devolver o ID (resolve C2)](#5--post-apialunos-devolver-o-id-resolve-c2)
- [§6 — Fluxo end-to-end](#6--fluxo-end-to-end)
- [Checklist de implementação](#checklist-de-implementação)
- [Referências](#referências)

---

## §0 — Visão geral e decisão de arquitetura

```
   Angular (browser)                    Host GestãoDeTurmas (:7048 HTTPS)
   ┌───────────────────┐   1. POST      ┌──────────── AlunosController ─────────────────────┐
   │ file-upload → CSV │  /api/alunos/  │  Importar(request)                                │
   │ parse + preview   ├───importar────►│    │  (GestãoDeTurmas/Controllers)                │
   │ (FRONT §5.2/5.3)  │  { alunos:[…] }│    ▼                                              │
   │ usuário confirma  │                │  IAlunoService.ImportarAlunosAsync (API/Service)  │
   │                   │                │   ├─ 2. valida CADA linha (nome/cpf/email/data/sexo)│
   │                   │◄──422 erros────┤   │     └─ alguma inválida ─► 422, NADA gravado    │
   │  destaca linhas   │  {indice,motivo}│  │                                              │
   │                   │                │   └─ 3. todas ok ─► IAlunoRepository.AdicionarVariosAsync
   │  pré-seleciona    │◄──201 criados──┤        (AddRange + 1 SaveChanges = atômico)       │
   │  alunosIds ◄──IDs─┤ {id,matricula} │        matrícula gerada por aluno (yyyyMM+hex)     │
   └───────────────────┘                └───────────────────────────────────────────────────┘
                       Camadas: GestãoDeTurmas (controllers/MVC) → API (services/DTOs)
                                → Repository (EF/repos/context) → Common (domínio/enums/utils)
```

**Decisão 1 — endpoint dedicado atômico, não N POSTs.** O front tem duas rotas (FRONT §5.4): **R1** um `POST /api/alunos/importar` atômico, **R2** um `forkJoin` de N `POST /api/alunos`. Adotamos **R1**. R2 não é atômico (falha na linha 40 deixa 39 alunos gravados), custa N round-trips e — pior no back atual — cada `POST` unitário faz um `SaveChanges` próprio ([`BaseRepository.AdicionarAsync`](../Repository/Repositories/Base/BaseRepository.cs)), sem transação entre eles. R1 valida e grava o lote com **um único `SaveChanges`**, que o EF já executa dentro de uma transação implícita: ou entra tudo, ou nada.

**Decisão 2 — recebe lista de itens (JSON), não o arquivo cru.** O CSV já foi parseado e **conferido pelo usuário no preview** (FRONT §5.3). Reparsear no servidor duplicaria lógica e quebraria o WYSIWYG. Recebemos a **lista de itens de aluno** — o mesmo formato do `POST /api/alunos` singular (`AlunoInputDTO`).

**Decisão 3 — atômico por padrão (422 se qualquer linha falhar).** Diante de "commitar as válidas e reportar as inválidas" (parcial) × "rejeitar o lote inteiro" (atômico), escolhemos **atômico**. O front já pré-valida e mostra as inválidas destacadas; devolver `422` com os erros por linha dá um ciclo limpo de _corrigir-e-reenviar_, sem estado meio-gravado. Importação **parcial** (`?parcial=true`) fica em backlog.

**Decisão 4 — validação e 422 são responsabilidade explícita do controller/service, não de infra global.** **Não há** conversão automática de exceção de negócio em `422`: o [`GlobalExceptionHandler`](../GestãoDeTurmas/Middlewares/GlobalExceptionHandler.cs) redireciona para páginas MVC (`/Home/Error`), o que quebraria um cliente JSON. O resultado "lote inválido" é produzido **explicitamente** no controller, com `try/catch` (ver §3 e §4). Como o `try/catch` do controller captura a exceção antes de ela "escapar", o `GlobalExceptionHandler` (que só roda em exceção **não tratada**, via `app.UseExceptionHandler()`) nunca a vê.

---

## Estado atual (verificado no código)

- ✅ **`POST /api/alunos` (criação unitária) existe** — [`AlunosController.AdicionarAluno`](../GestãoDeTurmas/Controllers/AlunosController.cs) recebe **`AlunoInputViewModel`**, chama `novoAluno.ToDTO()` ([mapper](../GestãoDeTurmas/Mappers/AlunoMapperExtensions.cs)) e delega a `IAlunoService.AdicionarAlunoAsync(AlunoInputDTO)`.
- ⚠️ **`POST /api/alunos` responde `Ok()` (200) sem corpo** — não devolve ID nem matrícula. É o **C2**. (Resolvido em §5.)
- ❌ **Não há `POST /api/alunos/importar`** — é o **C1**; criado em §4.
- ⚠️ **Não há GET-by-id de aluno** — o `AlunosController` só tem `[HttpGet]` de listagem. O `IAlunoService` tem `ObterPeloIdAsync`, mas nenhuma _action_ o expõe. Por isso `CreatedAtAction(nameof(ObterPorId))` **não é usável** — usamos `Created(url, dto)` (§4/§5).
- ✅ **Contrato do item (real):** [`AlunoInputDTO`](../API/DTOs/AlunoDTOs/AlunoInputDTO.cs) `{ string Nome; string Cpf; string? Email; DateOnly DataNascimento; SexoEnum Sexo }` — **sem DataAnnotations** (as annotations vivem no ViewModel, não no DTO).
- ✅ **`SexoEnum`** ([`Common/Enums/SexoEnum.cs`](../Common/Enums/SexoEnum.cs)) = `Masculino=1, Feminino=2, Outro=3`. No banco é **string** (`HasConversion<string>()`) com _check constraint_ `IN ('Masculino','Feminino','Outro')`.
- ✅ **Entidade `Aluno`** ([`Common/Domains/Aluno.cs`](../Common/Domains/Aluno.cs)) tem `Matricula` gerada pelo servidor. Mapeamento EF em [`AlunoConfiguration`](../Repository/Mappings/AlunoConfiguration.cs): índices **únicos** em `Cpf`, `Matricula` **e** `Email`; `Cpf` é `CHAR(11)` só-dígitos; `Email` é `VARCHAR(150)` nullable.
- ✅ **CPF e matrícula já têm utilitários** — [`ValidacaoCpf`](../Common/Utils/ValidacaoCpf.cs) e [`AlunoService.GerarMatriculaUnicaAsync`](../API/Service/AlunoService.cs). Reaproveitar.
- ⚠️ **Erro NÃO é um `ProblemDetails` limpo** — o `GlobalExceptionHandler` **redireciona** (302) para MVC. Os controllers de API contornam com `try/catch` devolvendo `500`. Como a classe é `[ApiController]`, **falhas de DataAnnotations viram `400` automático** — mas o import **não tem** DataAnnotations (vincula `AlunoInputDTO`), então toda validação é manual (§2).
- ✅ **Infra:** EF Core 10, `GestaoEscolarContext`, DI escopada em [`Program.cs`](../GestãoDeTurmas/Program.cs) (`IAlunoRepository`/`IAlunoService` já registrados — os métodos novos entram nas **mesmas** classes, sem DI nova), CORS `PermitirAngular` (`https://localhost:4200`, `AllowCredentials`), JWT via cookie `access_token`.

---

## §1 — Contrato: request e response da importação

### (A) Implementação

Estes cinco arquivos **já existem** no repositório (`API/DTOs/AlunoDTOs/`). Confira que batem com o abaixo:

```csharp
// [JÁ EXISTE] API/DTOs/AlunoDTOs/ImportarAlunosRequest.cs
namespace API.DTOs.AlunoDTOs;

public class ImportarAlunosRequest
{
    // Decisão fechada: vincula o DTO puro (sem DataAnnotations) → sem 400 automático.
    // Toda validação por linha acontece no service (§2), produzindo 422 uniforme.
    public IReadOnlyList<AlunoInputDTO> Alunos { get; set; } = [];
}
```

```csharp
// [JÁ EXISTE] API/DTOs/AlunoDTOs/AlunoCriadoDTO.cs
namespace API.DTOs.AlunoDTOs;

// Resposta de sucesso, por aluno. Cpf volta como âncora de conferência (linha↔criado).
public record AlunoCriadoDTO(int Id, string Matricula, string Cpf);
```

```csharp
// [JÁ EXISTE] API/DTOs/AlunoDTOs/ImportacaoResultadoDTO.cs
namespace API.DTOs.AlunoDTOs;

// ATENÇÃO: o campo está no SINGULAR (TotalCriado). No JSON sai "totalCriado".
public record ImportacaoResultadoDTO(int TotalCriado, IReadOnlyList<AlunoCriadoDTO> Criados);
```

```csharp
// [JÁ EXISTE] API/DTOs/AlunoDTOs/LinhaErroDTO.cs
namespace API.DTOs.AlunoDTOs;

// Erro POR LINHA, ancorado no índice do array enviado. Motivo é chave i18n, não frase.
public record LinhaErroDTO(int Indice, string Campo, string Motivo);
```

```csharp
// [JÁ EXISTE] API/DTOs/AlunoDTOs/ImportacaoErroDTO.cs
namespace API.DTOs.AlunoDTOs;

public record ImportacaoErroDTO(IReadOnlyList<LinhaErroDTO> Erros);
```

### (B) Conceitos

- **Ambiguidade resolvida — qual tipo o controller vincula.** O import vincula **`List<AlunoInputDTO>`** (via `ImportarAlunosRequest.Alunos`), **não** `List<AlunoInputViewModel>`. Consequência: como o DTO **não** tem DataAnnotations, o `[ApiController]` **não** gera `400` automático — logo **toda** validação de formato (nome 3–100, CPF, email) vira regra de negócio no service (§2). O custo é replicar no service o que hoje mora nas annotations do ViewModel; o ganho é um único formato de erro — `422 { indice, campo, motivo }` — ancorado no índice, que é o que o front espera. Essa duplicação é **intencional**.
- **A resposta carrega `Id` + `Matricula` por aluno** — resolve o **C2 no nível do lote**: o front injeta os IDs em `alunosIds` sem re-buscar. O `Cpf` devolvido é o **limpo** (11 dígitos), como gravado.
- **`TotalCriado` (singular).** O record foi criado no singular. Este doc mantém esse nome para bater com o código; o JSON sai como `totalCriado`. Se preferir o plural, renomeie o record **e** alinhe o front (o consumo ainda não existe — FRONT §5.4 —, então renomear agora é barato).

### (C) Comunicação com o front

1. O front parseia o CSV em `LinhaImportacao[]` (FRONT §5.2) e envia **só as linhas com `valida: true`** como itens de aluno.
2. Sucesso → `201` com `ImportacaoResultadoDTO`. O front faz `alunosIds.setValue(criados.map(c => c.id))` (FRONT §5.4, ⛔ não implementado).
3. Rejeição → `422` com `ImportacaoErroDTO`. O front cruza `Indice` com o preview e pinta a linha (classe `.invalida` de FRONT §5.3).
4. **Formato do CPF:** o back grava 11 dígitos e a validação (§2) faz `ValidacaoCpf.Limpar` antes de validar, então aceita CPF **com ou sem** máscara. Alinhe com o front o que o CSV traz.

---

## §2 — Validação server-side por linha

### (A) Implementação

Três blocos: (1) o helper de email, (2) a validação por linha, (3) as duas consultas de existência em lote no repositório.

#### 2.1 — Helper de email `[JÁ EXISTE]`

```csharp
// [JÁ EXISTE] API/Service/AlunoService.cs  — requer: using System.ComponentModel.DataAnnotations;
//
// Mesmo validador por trás do [EmailAddress] do AlunoInputViewModel. Existe aqui porque o
// import vincula AlunoInputDTO (sem DataAnnotations), então não há 400 automático de formato.
private static bool EmailBemFormado(string email) =>
    new EmailAddressAttribute().IsValid(email);
```

#### 2.2 — Validação por linha `[JÁ EXISTE]`

Devolve `null` se a linha é válida, ou o **primeiro** `LinhaErroDTO` encontrado. Recebe quatro `ISet<string>`: os dois de dedupe **intra-lote** (`cpfsNoLote`/`emailsNoLote`, que ela **alimenta** conforme percorre) e os dois de dedupe **inter-lote** (`cpfsNoBanco`/`emailsNoBanco`, pré-carregados do banco em 2.3).

```csharp
// [JÁ EXISTE] API/Service/AlunoService.cs
private LinhaErroDTO? ValidarLinha(
    AlunoInputDTO a, int indice,
    ISet<string> cpfsNoLote, ISet<string> emailsNoLote,
    ISet<string> cpfsNoBanco, ISet<string> emailsNoBanco)
{
    if (string.IsNullOrWhiteSpace(a.Nome) || a.Nome.Trim().Length is < 3 or > 100)
        return new(indice, nameof(a.Nome), "NOME_INVALIDO");

    // Sexo chega como SexoEnum; o System.Text.Json aceita um número fora de 1|2|3
    // sem reclamar → sem esta checagem, um 99 só cairia na check constraint do banco (500).
    if (!Enum.IsDefined(a.Sexo))
        return new(indice, nameof(a.Sexo), "SEXO_INVALIDO");

    // DataNascimento já é DateOnly (o binder converte); validar REGRA, não parse.
    var hoje = DateOnly.FromDateTime(DateTime.Today);
    if (a.DataNascimento >= hoje || a.DataNascimento < hoje.AddYears(-120))
        return new(indice, nameof(a.DataNascimento), "DATA_INVALIDA");

    // Email é OPCIONAL (string?). Só valida/deduplica se veio preenchido.
    if (!string.IsNullOrWhiteSpace(a.Email))
    {
        if (!EmailBemFormado(a.Email))
            return new(indice, nameof(a.Email), "EMAIL_INVALIDO");
        var email = a.Email.Trim().ToLowerInvariant();   // normaliza p/ casar com o set do banco
        if (emailsNoBanco.Contains(email))
            return new(indice, nameof(a.Email), "EMAIL_JA_EXISTE");
        if (!emailsNoLote.Add(email))                    // Add devolve false se já estava no set
            return new(indice, nameof(a.Email), "EMAIL_DUPLICADO_NO_LOTE");
    }

    var cpf = ValidacaoCpf.Limpar(a.Cpf);                // util JÁ existente (tira máscara)
    if (!ValidacaoCpf.IsCpfValido(cpf))                  // util JÁ existente (dígito verificador)
        return new(indice, nameof(a.Cpf), "CPF_INVALIDO");
    if (cpfsNoBanco.Contains(cpf))
        return new(indice, nameof(a.Cpf), "CPF_JA_EXISTE");
    if (!cpfsNoLote.Add(cpf))
        return new(indice, nameof(a.Cpf), "CPF_DUPLICADO_NO_LOTE");

    return null;
}
```

#### 2.3 — Existência em lote no repositório `[NOVO]`

O `IAlunoRepository` atual só tem `ExistePeloCpfAsync(cpf)` / `ExistePeloEmailAsync(email)` **unitários** — usá-los em loop seria N+1. Como o service **não** acessa o `DbContext`, precisamos expor duas consultas em lote (um `IN` cada). Adicione à interface e à implementação:

```csharp
// [ALTERAR] Repository/Repositories/AlunoRepository/IAlunoRepository.cs
// Acrescente estas duas assinaturas à interface existente:
Task<HashSet<string>> ObterCpfsExistentesAsync(IEnumerable<string> cpfs);
Task<HashSet<string>> ObterEmailsExistentesAsync(IEnumerable<string> emails);
```

```csharp
// [NOVO] Repository/Repositories/AlunoRepository/AlunoRepository.cs
// (dentro da classe AlunoRepository, onde _dbSet é acessível)

public async Task<HashSet<string>> ObterCpfsExistentesAsync(IEnumerable<string> cpfs)
{
    // CPFs já vêm limpos (11 dígitos) do service. Distinct evita repetir parâmetro no IN.
    var lista = cpfs.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
    if (lista.Count == 0)
        return new HashSet<string>();

    var encontrados = await _dbSet
        .Where(a => lista.Contains(a.Cpf))   // EF traduz para WHERE Cpf IN (@p0, @p1, ...)
        .Select(a => a.Cpf)
        .ToListAsync();

    return encontrados.ToHashSet();
}

public async Task<HashSet<string>> ObterEmailsExistentesAsync(IEnumerable<string> emails)
{
    // Normaliza (trim + lower) para casar com o que o service usa no dedupe.
    var normalizados = emails
        .Where(e => !string.IsNullOrWhiteSpace(e))
        .Select(e => e.Trim().ToLowerInvariant())
        .Distinct()
        .ToList();
    if (normalizados.Count == 0)
        return new HashSet<string>();

    // a.Email.ToLower() vira LOWER(Email) no SQL → comparação independe da collation do banco.
    var encontrados = await _dbSet
        .Where(a => a.Email != null && normalizados.Contains(a.Email.ToLower()))
        .Select(a => a.Email!)
        .ToListAsync();

    return encontrados.Select(e => e.Trim().ToLowerInvariant()).ToHashSet();
}
```

### (B) Conceitos

- **A validação de verdade é aqui, não no front.** O `valida` do front é só **usabilidade**. O servidor é a fronteira: CPF com dígito verificador ([`IsCpfValido`](../Common/Utils/ValidacaoCpf.cs)), email bem-formado, data dentro da regra (não futura, ≤120 anos — igual ao fluxo unitário em [`ValidarDataNascimento`](../API/Service/AlunoService.cs)), `sexo` dentro do enum. Nunca confie no cliente.
- **`Enum.IsDefined(a.Sexo)` é obrigatório no import.** O `System.Text.Json` desserializa um número fora de `1|2|3` para um `SexoEnum` inválido sem reclamar. O fluxo unitário não faz essa checagem (depende do `[Required]` do ViewModel + a _check constraint_); no import, sem DataAnnotations, ela é indispensável — senão um `99` só estouraria como `500` no `SaveChanges`.
- **Dedupe em DOIS campos e DOIS níveis.** `Cpf` **e** `Email` têm índice único ([`AlunoConfiguration`](../Repository/Mappings/AlunoConfiguration.cs)). Duplicata em qualquer um estoura o `SaveChanges` e derruba o lote com erro opaco. Por isso deduplicamos **intra-lote** (`HashSet` em memória, alimentado por `ValidarLinha`) e **inter-lote** (as duas consultas de 2.3). Email só quando preenchido.
- **Por que `HashSet<string>` e não `List`.** `ValidarLinha` faz `Contains` por linha; `HashSet` é O(1). E `HashSet` implementa `ISet<string>`, então o retorno do repositório entra direto nos parâmetros de `ValidarLinha`.
- **Limite do `IN`.** O `Where(lista.Contains(...))` vira um `IN (...)` com um parâmetro por item. O SQL Server tem teto de ~2100 parâmetros por comando. O limite de lote do §4 (`LIMITE_IMPORTACAO_ALUNOS = 1000`) mantém a consulta bem abaixo desse teto — os dois limites se protegem.
- **Atômico:** basta **uma** linha inválida para abortar o lote com `422` (Decisão 3). O service acumula **todos** os erros antes de responder (ver §3), evitando o "corrige um, descobre o próximo".

### (C) Comunicação com o front

- Os `Motivo`s (`CPF_INVALIDO`, `EMAIL_JA_EXISTE`, `DATA_INVALIDA`, `SEXO_INVALIDO`, `NOME_INVALIDO`, `CPF_DUPLICADO_NO_LOTE`, `EMAIL_DUPLICADO_NO_LOTE`, `CPF_JA_EXISTE`) são **chaves estáveis**, não frases. O front traduz via i18n. `CPF_JA_EXISTE` (no banco) vs. `CPF_DUPLICADO_NO_LOTE` (repetido na própria planilha) são distintos e úteis de diferenciar na UI.
- O front pode manter o `valida` local como filtro de UX, mas a palavra final é o `422`.

---

## §3 — Persistência atômica (tudo-ou-nada) e camada de repositório

### (A) Implementação

Quatro blocos: (1) a exceção de lote inválido, (2) o `AdicionarVariosAsync` no repositório, (3) o gerador de matrícula estendido, (4) o `ImportarAlunosAsync` completo + a entrada na interface.

#### 3.1 — `ImportacaoInvalidaException` `[NOVO]` — **em `API`, não em `Common`**

A exceção carrega `IReadOnlyList<LinhaErroDTO>`, e `LinhaErroDTO` mora em `API.DTOs.AlunoDTOs`. Como `Common` **não** referencia `API` (ver o diagrama de camadas no topo), colocá-la em `Common` **não compila**. Ela vai para o projeto `API`:

```csharp
// [NOVO] API/Exceptions/ImportacaoInvalidaException.cs
using API.DTOs.AlunoDTOs;

namespace API.Exceptions;

// Sinaliza "lote rejeitado" carregando os erros por linha. Capturada no controller (§4);
// NÃO sobe para o GlobalExceptionHandler (que redirecionaria para /Home/Error).
public class ImportacaoInvalidaException : Exception
{
    public IReadOnlyList<LinhaErroDTO> Erros { get; }

    public ImportacaoInvalidaException(IReadOnlyList<LinhaErroDTO> erros)
        : base("O lote de importação contém linhas inválidas.")
    {
        Erros = erros;
    }
}
```

#### 3.2 — `AdicionarVariosAsync` `[NOVO]` no repositório

O `AlunoService` não vê o `DbContext`; o `AddRange` + `SaveChanges` é exposto por um método novo:

```csharp
// [ALTERAR] Repository/Repositories/AlunoRepository/IAlunoRepository.cs
Task AdicionarVariosAsync(IEnumerable<Aluno> alunos);
```

```csharp
// [NOVO] Repository/Repositories/AlunoRepository/AlunoRepository.cs
public async Task AdicionarVariosAsync(IEnumerable<Aluno> alunos)
{
    _dbSet.AddRange(alunos);
    await _context.SaveChangesAsync();   // UM SaveChanges = transação implícita do EF (atômico)
}
```

#### 3.3 — Gerador de matrícula estendido `[ALTERAR]`

Hoje o gerador ([`GerarMatriculaUnicaAsync`](../API/Service/AlunoService.cs)) checa unicidade **só no banco**. Num lote, dois alunos podem sortear a mesma matrícula **antes** de qualquer `SaveChanges` — o índice único derrubaria a gravação. A correção é passar um `ISet<string>` das já sorteadas neste lote e checar contra o **banco E o set**. Mantemos uma sobrecarga sem argumento para o fluxo unitário não mudar:

```csharp
// [ALTERAR] API/Service/AlunoService.cs
// Sobrecarga sem argumento (usada pelo fluxo unitário) delega com um set descartável:
private Task<string> GerarMatriculaUnicaAsync() =>
    GerarMatriculaUnicaAsync(new HashSet<string>());

// Versão de lote: evita colisão intra-lote além da checagem no banco.
private async Task<string> GerarMatriculaUnicaAsync(ISet<string> usadasNoLote)
{
    string prefixo = DateTime.Now.ToString("yyyyMM");
    for (int tentativa = 0; tentativa < 5; tentativa++)
    {
        string aleatorio = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        string matricula = $"{prefixo}{aleatorio}";

        if (usadasNoLote.Contains(matricula))      // já sorteada neste lote → tenta de novo
            continue;

        if (!await _alunoRepository.ExisteMatriculaAsync(matricula))
        {
            usadasNoLote.Add(matricula);           // reserva para os próximos do lote
            return matricula;
        }
    }
    throw new RegraDeNegocioException("Não foi possível gerar uma matrícula única. Tente novamente!!");
}
```

> A chamada existente `Matricula = await GerarMatriculaUnicaAsync()` em `AdicionarAlunoAsync` continua válida — resolve para a sobrecarga sem argumento. **Nenhuma outra alteração no fluxo unitário.**

#### 3.4 — `ImportarAlunosAsync` `[NOVO]` + entrada na interface

```csharp
// [ALTERAR] API/Service/IAlunoService.cs  — requer: using API.DTOs.AlunoDTOs;
Task<ImportacaoResultadoDTO> ImportarAlunosAsync(ImportarAlunosRequest request);
```

```csharp
// [NOVO] API/Service/AlunoService.cs  — requer: using API.Exceptions;
public async Task<ImportacaoResultadoDTO> ImportarAlunosAsync(ImportarAlunosRequest request)
{
    var itens = request.Alunos;

    // 1) carrega o "já existe no banco" em lote (2 consultas), monta os sets de dedupe inter-lote
    var cpfsBanco = await _alunoRepository.ObterCpfsExistentesAsync(
        itens.Select(i => ValidacaoCpf.Limpar(i.Cpf)));
    var emailsBanco = await _alunoRepository.ObterEmailsExistentesAsync(
        itens.Where(i => !string.IsNullOrWhiteSpace(i.Email))
             .Select(i => i.Email!));

    // 2) valida CADA linha (§2), acumulando TODOS os erros (não para no primeiro)
    var erros = new List<LinhaErroDTO>();
    var cpfsLote = new HashSet<string>();
    var emailsLote = new HashSet<string>();
    for (int i = 0; i < itens.Count; i++)
    {
        var erro = ValidarLinha(itens[i], i, cpfsLote, emailsLote, cpfsBanco, emailsBanco);
        if (erro is not null)
            erros.Add(erro);
    }
    if (erros.Count > 0)
        throw new ImportacaoInvalidaException(erros);   // tratada no controller (§4), NÃO no handler global

    // 3) mapeia p/ entidade + gera matrícula única por aluno (sem colidir no lote)
    var matriculasUsadas = new HashSet<string>();
    var alunos = new List<Aluno>(itens.Count);
    foreach (var item in itens)
    {
        alunos.Add(new Aluno
        {
            Nome = item.Nome.Trim(),
            Cpf = ValidacaoCpf.Limpar(item.Cpf),
            Email = item.Email?.Trim(),
            DataNascimento = item.DataNascimento,
            Sexo = item.Sexo,
            Ativo = true,
            Matricula = await GerarMatriculaUnicaAsync(matriculasUsadas)
        });
    }

    // 4) grava tudo numa transação (um SaveChanges) — os Id identity só existem DEPOIS disso
    await _alunoRepository.AdicionarVariosAsync(alunos);

    var criados = alunos
        .Select(a => new AlunoCriadoDTO(a.Id, a.Matricula, a.Cpf))
        .ToList();
    return new ImportacaoResultadoDTO(criados.Count, criados);
}
```

### (B) Conceitos

- **Um `SaveChanges` já é atômico.** Com `AddRange` + **um** `SaveChangesAsync`, o EF envia os inserts numa transação implícita — ou tudo entra, ou nada. `BeginTransaction` explícito só é necessário com múltiplos `SaveChanges` ou SQL cru; não é o caso. Se o `SaveChanges` estourar (ex.: corrida no índice único), o EF reverte sozinho — nenhum aluno meio-criado.
- **`SaveChanges` popula os `Id`.** Colunas _identity_ só têm valor **depois** do insert; por isso `AlunoCriadoDTO` é montado **após** `AdicionarVariosAsync`. Montar antes devolveria `Id = 0`.
- **Matrícula: custo de round-trip.** `GerarMatriculaUnicaAsync` faz uma consulta `ExisteMatriculaAsync` por tentativa/aluno. Para lotes grandes são muitos round-trips. Aceitável para import; se virar gargalo, a otimização é pré-carregar num `HashSet` todas as matrículas com o prefixo `yyyyMM` atual (uma consulta) e gerar 100% em memória contra esse set + `usadasNoLote`. Fica como melhoria opcional; a versão acima é correta e simples.
- **Erro de lote → exceção, capturada no controller.** Escolhemos **`ImportacaoInvalidaException`** (em `API`) em vez de retornar um resultado-união, para manter o retorno do service limpo (`ImportacaoResultadoDTO` = só sucesso) e alinhado à convenção do codebase (o fluxo unitário e o MVC [`GerenciarAlunoController`](../GestãoDeTurmas/Controllers/GerenciarAlunoController.cs) já usam exceção de negócio + `catch`). A alternativa (service devolver `{ Erros | Criados }` e o controller ramificar) é válida, mas **não** é a adotada aqui — não misturar as duas.

### (C) Comunicação com o front

- Só há `201` **se o `SaveChanges` passou** — o front nunca recebe IDs de algo que não gravou. Isso torna o `alunosIds.setValue(...)` seguro.
- **Resolve o C1** integralmente: o endpoint existe, é atômico e devolve os IDs.

---

## §4 — Endpoint `POST /api/alunos/importar`

### (A) Implementação

Primeiro, o limite de lote como constante (o front limita o arquivo a 2 MB — FRONT §5.3 —, mas o servidor não confia nisso):

```csharp
// [ALTERAR] Common/Constantes.cs
public static class Constantes
{
    public const int TAMANHO_PAGINA = 50;            // já existe (paginação, não import)
    public const int LIMITE_IMPORTACAO_ALUNOS = 1000; // NOVO: teto de linhas por importação
}
```

A _action_ no controller (mesma classe do `POST` unitário):

```csharp
// [NOVO] GestãoDeTurmas/Controllers/AlunosController.cs
// usings no topo do arquivo: using API.DTOs.AlunoDTOs;  using API.Exceptions;  using Common;
[HttpPost("importar")]
[Authorize(Roles = "Admin,Coordenador")]  // ver NOTA de roles em (B)
[ProducesResponseType(typeof(ImportacaoResultadoDTO), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ImportacaoErroDTO), StatusCodes.Status422UnprocessableEntity)]
public async Task<IActionResult> Importar([FromBody] ImportarAlunosRequest request)
{
    if (request?.Alunos is null || request.Alunos.Count == 0)
        return BadRequest("LOTE_VAZIO");

    if (request.Alunos.Count > Constantes.LIMITE_IMPORTACAO_ALUNOS)
        return BadRequest("LOTE_MUITO_GRANDE");

    try
    {
        var resultado = await _alunoService.ImportarAlunosAsync(request);
        return Created($"/api/alunos", resultado);  // 201 + corpo; sem depender de GET-by-id
    }
    catch (ImportacaoInvalidaException ex)
    {
        // 422 explícito — NÃO deixe subir para o GlobalExceptionHandler (que redireciona).
        return UnprocessableEntity(new ImportacaoErroDTO(ex.Erros));
    }
    catch (Exception)
    {
        return StatusCode(500, mensagemStatus500);  // mesmo campo privado já usado nos outros endpoints
    }
}
```

### (B) Conceitos

- **Rota `POST /api/alunos/importar`** — o controller usa `[Route("api/[controller]")]`, então o prefixo `api/` é obrigatório. Sub-recurso de `alunos`, sem controller novo.
- **NOTA de roles — como `[Authorize]` empilha (isto é uma decisão, não "override").** A **classe** `AlunosController` é `[Authorize(Roles = "Admin,Coordenador,Docente")]`. Múltiplos atributos `[Authorize]` (classe + método) são avaliados em **AND** — o usuário precisa satisfazer **todos**; dentro de um mesmo atributo, os papéis são **OR**. Logo o efeito é a **interseção**:
  - classe: `Admin ∨ Coordenador ∨ Docente`
  - método: `Admin ∨ Coordenador`
  - efetivo: **`Admin ∨ Coordenador`** → `Docente` fica de fora **do import** (mas continua criando no unitário).
  Ou seja, o atributo no método **não "sobrepõe"** o da classe; ele **restringe** por interseção — o resultado é o que queremos. **Decisão adotada:** importação só para `Admin,Coordenador`. Para manter paridade com o unitário (incluir `Docente`), troque a linha para `[Authorize(Roles = "Admin,Coordenador,Docente")]`. Papéis são strings livres (`Usuario.Role`); o seed cria só `Admin` ([`Program.cs`](../GestãoDeTurmas/Program.cs)).
- **`422` (inválido), `400` (forma), `201` (ok).** Lote vazio ou acima do limite é erro de **forma** (`400`); linhas inválidas são **semânticas** (`422`, explícito no `catch`). O `Created(url, corpo)` devolve `201` com corpo e um header `Location` — sem precisar de uma _action_ GET-by-id (que não existe). Usar `CreatedAtAction(nameof(ObterPorId))` lançaria em runtime.
- **Autenticação vem de cookie.** O JWT é lido de `Request.Cookies["access_token"]` ([`Program.cs`](../GestãoDeTurmas/Program.cs)), não de header `Authorization`. O CORS `PermitirAngular` libera `AllowCredentials` para `https://localhost:4200` — o front precisa enviar `withCredentials: true`.
- **Nenhuma DI nova.** `IAlunoService`/`IAlunoRepository` já estão registrados; os métodos novos entram nas classes já registradas.

### (C) Comunicação com o front

1. O front adiciona `AlunoService.importarAlunos(...)` → `POST {apiUrl}/api/alunos/importar` (com `withCredentials`).
2. `AlunoFacadeService.importarAlunos(linhas)` filtra `valida`, mapeia e chama o service.
3. O container do wizard consome e faz `alunosIds.setValue(resultado.criados.map(c => c.id))` — FRONT §5.4, **⛔ ainda não implementado** (dependia deste endpoint).

---

## §5 — `POST /api/alunos` devolver o ID (resolve C2)

### (A) Implementação

Hoje o fluxo unitário é `void` de ponta a ponta: [`AlunosController.AdicionarAluno`](../GestãoDeTurmas/Controllers/AlunosController.cs) retorna `Ok()`, e [`IAlunoService.AdicionarAlunoAsync`](../API/Service/IAlunoService.cs) retorna `Task`. Três mudanças:

```csharp
// [ALTERAR] API/Service/IAlunoService.cs — passar a devolver o aluno criado
Task<Aluno> AdicionarAlunoAsync(AlunoInputDTO aluno);   // hoje: Task
```

```csharp
// [ALTERAR] API/Service/AlunoService.cs — mudar o tipo de retorno e devolver a entidade
public async Task<Aluno> AdicionarAlunoAsync(AlunoInputDTO aluno)
{
    await ValidarDadosAlunoAsync(aluno.DataNascimento, aluno.Email, aluno.Sexo);
    var cpfLimpo = await ValidacaoCpf.ValidarEProcessarCpfAsync(aluno.Cpf, _alunoRepository.ExistePeloCpfAsync);

    Aluno novoAluno = new Aluno
    {
        Nome = aluno.Nome,
        Cpf = cpfLimpo,
        Email = aluno.Email,
        DataNascimento = aluno.DataNascimento,
        Sexo = aluno.Sexo,
        Ativo = true,
        Matricula = await GerarMatriculaUnicaAsync()
    };

    await _alunoRepository.AdicionarAsync(novoAluno);
    return novoAluno;   // Id/Matricula já populados pelo SaveChanges de AdicionarAsync
}
```

```csharp
// [ALTERAR] GestãoDeTurmas/Controllers/AlunosController.cs — 201 + corpo (era Ok())
[HttpPost]
public async Task<IActionResult> AdicionarAluno([FromBody] AlunoInputViewModel novoAluno)
{
    try
    {
        var aluno = await _alunoService.AdicionarAlunoAsync(novoAluno.ToDTO());
        var dto = new AlunoCriadoDTO(aluno.Id, aluno.Matricula, aluno.Cpf);
        return Created($"/api/alunos/{aluno.Id}", dto);
    }
    catch (RegraDeNegocioException ex)
    {
        return UnprocessableEntity(ex.Message);   // hoje isso vira 500 (é engolido pelo catch genérico)
    }
    catch (Exception)
    {
        return StatusCode(500, mensagemStatus500);
    }
}
```

### (B) Conceitos

- **De `Ok()`/void para `201 + corpo`.** Passar a devolver `AlunoCriadoDTO` **resolve o C2** também no caminho unitário.
- **A mudança de assinatura é compatível com os chamadores atuais.** `AdicionarAlunoAsync` é chamado em 3 lugares, todos apenas `await` sem usar o retorno — mudar `Task` → `Task<Aluno>` **compila sem tocar neles**:
  - [`AlunosController.AdicionarAluno`](../GestãoDeTurmas/Controllers/AlunosController.cs) (API) — alterado acima para **usar** o retorno.
  - [`GerenciarAlunoController.Adicionar`](../GestãoDeTurmas/Controllers/GerenciarAlunoController.cs) (MVC) — `await ...;` continua válido (ignora o `Aluno`).
  - [`Tests/AlunoServiceTests.cs`](../Tests/AlunoServiceTests.cs) (4 casos) — `Func<Task> acao = async () => await ...;` continua válido (uma lambda `async` que faz `await` de um `Task<Aluno>` é atribuível a `Func<Task>`).
- **`CreatedAtAction` NÃO é aplicável hoje** — não existe _action_ GET-by-id. Por isso usamos `Created($"/api/alunos/{id}", dto)`: dá o `201` + `Location` sem depender de uma action inexistente. (Se um dia criar `[HttpGet("{id}")] ObterPorId` usando o `ObterPeloIdAsync` que já existe no service, aí pode migrar para `CreatedAtAction`.)
- **Erro de negócio hoje vira 500.** O `catch (Exception)` atual engole `RegraDeNegocioException` como `500`. A mudança acima captura `RegraDeNegocioException` → `422`, deixando `500` só para o inesperado.
- **Compatibilidade com o front:** adicionar corpo a um `201` é retrocompatível; só exige o front **atualizar o tipo** de `void` para `AlunoCriadoDTO`.

### (C) Comunicação com o front

- Front muda `adicionarAluno(...): Observable<AlunoCriadoDTO>` e, onde hoje ignora o retorno, pode usar o `id` (ex.: pré-selecionar após cadastro unitário).

---

## §6 — Fluxo end-to-end

```
usuário confere o preview de "turma-3a.csv" e clica "confirmar":
  front envia POST /api/alunos/importar  { alunos: [ {nome,cpf,email,dataNascimento,sexo}, … ] }
        │  (withCredentials → cookie access_token)
        ▼  AlunosController.Importar
  400 se vazio (LOTE_VAZIO) ou > 1000 linhas (LOTE_MUITO_GRANDE)
        ▼  AlunoService.ImportarAlunosAsync
  pré-carrega CPFs/emails já no banco (ObterCpfsExistentesAsync / ObterEmailsExistentesAsync — 2 IN)
  §2 valida CADA linha (nome/sexo/data/email/cpf) + dedupe intra-lote (CPF e EMAIL), acumulando erros
        ├─ alguma inválida ─► throw ImportacaoInvalidaException
        │                     └─► controller: 422 { erros:[ {indice:3, campo:"Cpf", motivo:"CPF_INVALIDO"} ] }
        │                         └─► front pinta a linha 3 ; NADA foi gravado
        ▼ todas ok
  §3 gera matrícula única por aluno (yyyyMM+hex, sem colidir no lote via HashSet)
     → IAlunoRepository.AdicionarVariosAsync (AddRange + 1 SaveChanges = atômico)
        │  (SaveChanges popula os Id identity)
        ▼
  201  { totalCriado: 30, criados: [ {id:101, matricula:"202607A3F2", cpf:"12345678901"}, … ] }
        │
        ▼  front: alunosIds.setValue(criados.map(c => c.id))  → importados já pré-selecionados
```

---

## Checklist de implementação

- [x] **§1 — DTOs** em [`API/DTOs/AlunoDTOs/`](../API/DTOs/AlunoDTOs/AlunoInputDTO.cs): `ImportarAlunosRequest`, `AlunoCriadoDTO`, `ImportacaoResultadoDTO` (campo `TotalCriado`), `LinhaErroDTO`, `ImportacaoErroDTO`. **Já existem no repositório** — só conferir os nomes.
- [x] **§2 — Validação** em [`AlunoService`](../API/Service/AlunoService.cs): `EmailBemFormado` (já adicionado) e `ValidarLinha` (já adicionado). **Novo no repositório:** `ObterCpfsExistentesAsync` e `ObterEmailsExistentesAsync` (interface + impl), um `IN` cada.
- [x] **§3 — Persistência:** `ImportacaoInvalidaException` em **`API/Exceptions/`** (não em `Common`); `AdicionarVariosAsync` em [`IAlunoRepository`/`AlunoRepository`](../Repository/Repositories/AlunoRepository/IAlunoRepository.cs) (`AddRange` + **um** `SaveChanges`); sobrecarga `GerarMatriculaUnicaAsync(ISet<string>)` (evita colisão intra-lote); `ImportarAlunosAsync` no service + assinatura na [`IAlunoService`](../API/Service/IAlunoService.cs); `AlunoCriadoDTO` montado **após** o `SaveChanges`.
- [x] **§4 — Endpoint** em [`AlunosController`](../GestãoDeTurmas/Controllers/AlunosController.cs): `[HttpPost("importar")]`; `201` via `Created(...)` / `422` via `catch` explícito / `400` (vazio ou > limite); roles `Admin,Coordenador` (interseção com a classe); constante `LIMITE_IMPORTACAO_ALUNOS` em [`Common/Constantes.cs`](../Common/Constantes.cs); auth por **cookie** + `AllowCredentials`.
- [x] **§5 — `POST /api/alunos` unitário:** `AdicionarAlunoAsync` passa a **retornar `Aluno`** (interface + impl); controller devolve `201` + `AlunoCriadoDTO` via `Created(...)`; capturar `RegraDeNegocioException` → `422`. Chamadores (MVC + testes) continuam compilando.
- [x] **Erro/infra:** ciente de que o [`GlobalExceptionHandler`](../GestãoDeTurmas/Middlewares/GlobalExceptionHandler.cs) **redireciona** (MVC) — o `422`/`400`/`500` da API sai do próprio controller.
- [ ] **Front (par, depende deste doc):** `AlunoService.importarAlunos` (`withCredentials`), `AlunoFacadeService.importarAlunos`, `alunosIds.setValue(...)`, atualizar tipo de `adicionarAluno` — FRONT §5.4, **⛔ só depois** deste back existir.
- [ ] **Smoke (Swagger, em `https://localhost:7048`):** lote 100% válido → `201` com N `criados` e matrículas distintas; 1 CPF inválido → `422` no `indice` certo; CPF repetido no lote → `422 CPF_DUPLICADO_NO_LOTE`; email repetido no lote → `422 EMAIL_DUPLICADO_NO_LOTE`; CPF/email já existente → `422 *_JA_EXISTE`; lote vazio → `400 LOTE_VAZIO`; lote > 1000 → `400 LOTE_MUITO_GRANDE`; conferir que um `422` **não** gravou nada no banco.

---

## Referências

### Back (este repositório — verificado)

- Entidade e enum: [`Common/Domains/Aluno.cs`](../Common/Domains/Aluno.cs), [`Common/Enums/SexoEnum.cs`](../Common/Enums/SexoEnum.cs).
- DTO e ViewModel de entrada: [`API/DTOs/AlunoDTOs/AlunoInputDTO.cs`](../API/DTOs/AlunoDTOs/AlunoInputDTO.cs), [`GestãoDeTurmas/Models/Aluno/AlunoInputViewModel.cs`](../GestãoDeTurmas/Models/Aluno/AlunoInputViewModel.cs), [mapper](../GestãoDeTurmas/Mappers/AlunoMapperExtensions.cs).
- DTOs da importação: [`ImportarAlunosRequest`](../API/DTOs/AlunoDTOs/ImportarAlunosRequest.cs), [`AlunoCriadoDTO`](../API/DTOs/AlunoDTOs/AlunoCriadoDTO.cs), [`ImportacaoResultadoDTO`](../API/DTOs/AlunoDTOs/ImportacaoResultadoDTO.cs), [`LinhaErroDTO`](../API/DTOs/AlunoDTOs/LinhaErroDTO.cs), [`ImportacaoErroDTO`](../API/DTOs/AlunoDTOs/ImportacaoErroDTO.cs).
- Service e interface: [`API/Service/AlunoService.cs`](../API/Service/AlunoService.cs), [`API/Service/IAlunoService.cs`](../API/Service/IAlunoService.cs).
- Repositório e base: [`AlunoRepository`](../Repository/Repositories/AlunoRepository/AlunoRepository.cs), [`IAlunoRepository`](../Repository/Repositories/AlunoRepository/IAlunoRepository.cs), [`BaseRepository`](../Repository/Repositories/Base/BaseRepository.cs).
- Controller, context e mapping: [`AlunosController`](../GestãoDeTurmas/Controllers/AlunosController.cs), [`GerenciarAlunoController`](../GestãoDeTurmas/Controllers/GerenciarAlunoController.cs), [`GestaoEscolarContext`](../Repository/Context/GestaoEscolarContext.cs), [`AlunoConfiguration`](../Repository/Mappings/AlunoConfiguration.cs).
- Utilitários, constantes e exceções: [`ValidacaoCpf`](../Common/Utils/ValidacaoCpf.cs), [`Constantes`](../Common/Constantes.cs), [`RegraDeNegocioException`](../Common/Exceptions/RegraDeNegocioException.cs), [`EntidadeNaoEncontradaException`](../Common/Exceptions/EntidadeNaoEncontradaException.cs).
- Host, DI e erro global: [`GestãoDeTurmas/Program.cs`](../GestãoDeTurmas/Program.cs), [`GlobalExceptionHandler`](../GestãoDeTurmas/Middlewares/GlobalExceptionHandler.cs).
- Convenções gerais: [SEGURANCA-ARQUITETURA.md](SEGURANCA-ARQUITETURA.md), [AUTENTICACAO-TOKENS.md](AUTENTICACAO-TOKENS.md).

### Front (outro repositório — textual)

- Contrato do item e enum: `../src/app/shared/interfaces/dto/aluno-adicionar-dto.interface.ts`, `../src/app/shared/enums/sexo.enum.ts`, `../src/app/shared/interfaces/entities/aluno.interface.ts`.
- Consumo no front: `../src/app/core/services/aluno.service.ts`, `../src/app/core/facades/aluno-facade.service.ts`.
- Documento par (front): `COMPONENTE-FILE-UPLOAD.md` — §5.1 leitura, §5.2 parse, §5.3 preview, §5.4 handoff (C1/C2).

### Externas

- [EF Core — transações](https://learn.microsoft.com/ef-core/saving/transactions) · [`AddRange`/`SaveChanges`](https://learn.microsoft.com/ef-core/saving/basic) · [HTTP 422 — MDN](https://developer.mozilla.org/docs/Web/HTTP/Status/422) · [Múltiplos `[Authorize]` (AND)](https://learn.microsoft.com/aspnet/core/security/authorization/roles) · [Tipos de retorno de action](https://learn.microsoft.com/aspnet/core/web-api/action-return-types) · [Comportamento do `[ApiController]`](https://learn.microsoft.com/aspnet/core/web-api/#apicontroller-attribute).
