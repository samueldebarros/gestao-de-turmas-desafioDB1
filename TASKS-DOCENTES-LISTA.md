# Tasks — Docentes: a lista fica operável (Fatia A)

> **O que é:** as unidades de execução. Cada uma é uma passada verificável isoladamente; se não dá para provar sozinha, são duas tasks.
>
> **Contrato em** [SPEC-DOCENTES-LISTA.md](SPEC-DOCENTES-LISTA.md) · **ordem e decisões em** [PLANO-DOCENTES-LISTA.md](PLANO-DOCENTES-LISTA.md). Este documento não repete nenhum dos dois — referencia `CA-n` e `D-n`.
>
> ⚠️ **Escrito em dois blocos, de propósito.** O bloco 1 é back-end e produz o JSON real da resposta. O bloco 2 (front) só é escrito **depois** disso: escrever agora o critério de aceite de "traduzir JSON em tipo" seria escrevê-lo sobre algo que ainda não existe — que é exatamente como o campo fantasma `totalCriados` nasceu.
>
> **Nenhuma task fica pronta por afirmação.** Pronta é o comando do critério de aceite fechando.

## Bloco 1 — Back-end (repositório `DesafioTecnico1-Fundamentos`)

### T1 — DTO achatado de listagem de docente

**Contexto mínimo:** `Docente.Disciplina` ↔ `Disciplina.Docentes` formam ciclo. Serializar a entidade estoura no `System.Text.Json`, e o `GlobalExceptionHandler` transforma isso num 302 para rota inexistente — o front receberia um 404 mudo. Decisão **D-1**.

**Arquivos-alvo:** um DTO novo em `API/DTOs/DocenteDTOs/`; projeção no `DocenteService` ou no `DocenteRepository`.

**O que fazer:** criar `DocenteListaDTO` com exatamente `Id`, `Nome`, `Email`, `DisciplinaNome`, `Ativo`. Projetar a partir da consulta existente, sem `Include` da navegação — ou com `Select` que achata antes de materializar.

⛔ **Não incluir** `Cpf`, `DataNascimento`, `DisciplinaId`, `CargaHoraria` nem o objeto `Disciplina` (**D-2**).

**Critério de aceite:** um endpoint de teste que devolva a projeção responde **200 com `application/json`**, não 302. O objeto tem exatamente cinco campos.

**Dependências:** nenhuma. **Rollback:** apagar o DTO e a projeção; nada mais consome.

**Estimativa:** 3h

### T2 — Enum de ordenação validado

**Contexto mínimo:** hoje `ordenacao` é `string` num `switch` case-sensitive **sem validação** — valor errado cai no default e ordena por nome em silêncio, devolvendo 200. Decisão **D-4**; espelha o que `AlunosController` já faz com `OrdenacaoAlunoEnum`.

**Arquivos-alvo:** um enum novo em `Common/Enums/`; o ponto de entrada que hoje recebe a string.

**O que fazer:** criar `OrdenacaoDocenteEnum` com `Nome = 1` e `Disciplina = 2`. Validar com `Enum.IsDefined` e devolver **400** para valor fora do intervalo, como em `AlunosController.BuscarAlunos`.

⛔ **Não acrescentar** outros valores mesmo que o repositório honre mais campos (**D-12**).

**Critério de aceite:** no Swagger, ordenação `1` e `2` respondem 200 e produzem ordens **diferentes** entre si; ordenação `99` responde **400**, e não uma lista ordenada por nome. Este último é o `CA-3`.

**Dependências:** nenhuma. **Rollback:** voltar a aceitar a string.

**Estimativa:** 2h

### T3 — `POST /api/docentes/buscar`

**Contexto mínimo:** `ListaPaginada<T>` herda de `List<T>` e serializa como array puro, descartando os metadados de paginação. O padrão do projeto é montar o envelope à mão no controller — três ocorrências idênticas em `AlunosController` e `TurmasController`. Decisões **D-3** e **D-8**.

**Arquivos-alvo:** `GestãoDeTurmas/Controllers/DocentesController.cs`; um request DTO em `API/DTOs/DocenteDTOs/`.

**O que fazer:** `DocenteBuscaRequest` com `Pagina`, `TamanhoPagina` (padrão **10**), `Pesquisa`, `Ativo`, `Ordenacao`, `Direcao`. O endpoint delega ao `ObterTodosOsDocentesAsync` já existente, projeta para `DocenteListaDTO` (T1) e devolve `Ok(new { itens = ..., PaginaAtual, TotalPaginas, TotalResultados, TamanhoPagina })`.

Adicionar `[ProducesResponseType(typeof(...), 200)]` — hoje o Swagger de docentes não declara schema nenhum.

⛔ **Não tocar** no `GET /api/docentes` existente: 13 arquivos do front dependem do formato achatado atual.

**Critério de aceite:** no Swagger, a busca responde 200 com os cinco campos do envelope preenchidos e `itens` como array de objetos de cinco campos (`CA-1`, `CA-2`). Filtro `ativo: null` traz todos; `true` e `false` trazem subconjuntos com contagens diferentes.

**Dependências:** T1, T2. **Rollback:** remover a action; o `GET` antigo segue intacto.

**Estimativa:** 4h

### T4 — `PATCH` inativar/reativar e a correção do vínculo

**Contexto mínimo:** `DocenteRepository.InativarAsync` faz `SetProperty(d => d.DisciplinaId, (int?)null)` — **apaga o vínculo com a disciplina** — e `ReativarAsync` não devolve. O docente reativado fica órfão e some do `GET /api/docentes`, que faz join com disciplina. Decisão **D-5**.

Esta fatia entrega justamente o botão que dispara essa perda; por isso a correção entra aqui e não vira dívida.

**Arquivos-alvo:** `DocentesController.cs`; `DocenteRepository.cs`.

**O que fazer:** expor `PATCH /api/docentes/{id}/inativar` e `/reativar` delegando aos métodos já existentes, no mesmo padrão de `AlunosController`. Remover do `InativarAsync` a limpeza do `DisciplinaId`.

⛔ **Não** criar regra de negócio bloqueando inativação de docente vinculado a turma — não existe hoje e não é escopo desta fatia.
⛔ **Não** diferenciar "não existe" de "já está nesse estado": o 404 ambíguo é aceito (**D-7**).

**Critério de aceite:** `CA-4` — um docente **com disciplina** é inativado e reativado, e volta com **a mesma disciplina**, reaparecendo no `GET /api/docentes`. Verificar no banco que `DisciplinaId` não ficou nulo em nenhum momento.

**Dependências:** nenhuma. **Rollback:** remover as actions; a correção do repositório pode ficar (é correção de bug, não feature).

**Estimativa:** 3h

### T5 — Capturar o contrato real

**Contexto mínimo:** é a ponte entre os dois blocos. Interface TypeScript é promessa sobre JSON, não validação — e este projeto já tem um campo fantasma vivo (`totalCriados` no front contra `totalCriado` no back) para provar o custo de supor.

**Arquivos-alvo:** nenhum de código. O entregável é registro.

**O que fazer:** chamar os três endpoints novos contra o banco real e **colar a resposta literal** — não a descrição dela — em `docs/desenvolvimento-com-ia/CONTRATO-DOCENTES-CAPTURADO.md`. Incluir: a busca com resultados, a busca vazia, a busca com ordenação inválida (400), e um inativar seguido de reativar.

**Critério de aceite:** o arquivo existe e cada campo do JSON aparece com o nome e o tipo exatos que saíram do servidor, incluindo o casing.

**Dependências:** T3, T4. **Rollback:** não se aplica.

**Estimativa:** 1h

**Total do bloco 1: 13h.**

## Bloco 2 — Front-end

⛔ **Ainda não escrito, e é decisão de método.** Será escrito depois da T5, contra o JSON capturado.

Escopo previsto, em ordem: contratos do front (`DocenteListaInterface`, `DocenteFiltro`, `OrdenacaoDocenteEnum`) → Service + spec → Facade + spec → Smart + i18n + spec → fechamento (zerar o teto de `R4` na allowlist, subir a catraca, percorrer os marcos `[olho]`).

Os critérios de aceite já estão fixados na spec (`CA-5` a `CA-17`) e não dependem do JSON — só os nomes de campo dependem. O motivo do adiamento é estreito e declarado: escrever hoje o critério de "a interface casa com a resposta real" seria escrevê-lo sobre algo que ainda não existe.
