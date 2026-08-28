# Tasks — Docentes: cadastro e edição (Fatia B)

> **O que é:** as unidades de execução. Cada uma é uma passada verificável isoladamente; se não dá para provar sozinha, são duas tasks.
>
> **Contrato em** [SPEC-DOCENTES-CADASTRO.md](SPEC-DOCENTES-CADASTRO.md) · **ordem e decisões em** [PLANO-DOCENTES-CADASTRO.md](PLANO-DOCENTES-CADASTRO.md). Este documento não repete nenhum dos dois — referencia `CA-n` e `D-n`. `A/D-n` e `A/T-n` são da Fatia A.
>
> ⚠️ **Escrito em dois blocos, de propósito.** O bloco 1 é back-end e produz o JSON real das respostas. O **bloco 2 (front) ainda não existe** e só será escrito depois disso: escrever agora o critério de aceite de "traduzir JSON em tipo" seria escrevê-lo sobre algo que não existe — que é exatamente como o campo fantasma `totalCriados` nasceu.
>
> **Nenhuma task fica pronta por afirmação.** Pronta é o comando do critério de aceite fechando.

## Índice

- [Bloco 1 — Back-end](#bloco-1--back-end-repositório-desafiotecnico1-fundamentos)
- [Bloco 2 — Front-end](#bloco-2--front-end--ainda-não-escrito)
- [Apêndice — prompt autocontido para a sessão do back-end](#apêndice--prompt-autocontido-para-a-sessão-do-back-end)

## Bloco 1 — Back-end (repositório `DesafioTecnico1-Fundamentos`)

⚠️ O front **não acessa** este repositório. As tasks abaixo são executadas por outra sessão, com o prompt do apêndice.

### T1 — `GET /api/disciplinas`

**Contexto mínimo:** não existe `DisciplinasController` REST — só o MVC `GerenciarDisciplinaController`, que é dev-only pelo guard de deploy. O `IDisciplinaService` já existe. É a **única peça da fatia sem nada pronto por baixo**, e por isso sobe primeiro (**D-7**): o formulário do front precisa do **id** da disciplina, e sem o endpoint o campo nasce contra um formato imaginado.

**Arquivos-alvo:** `GestãoDeTurmas/Controllers/DisciplinasController.cs` (novo); um DTO em `API/DTOs/`.

**O que fazer:** um `GET /api/disciplinas` devolvendo array de objetos com **`Id` e `Nome`**, protegido pelos mesmos papéis do `DocentesController`. Declarar `[ProducesResponseType(typeof(...), 200)]`.

⛔ **Não** paginar: é lista de apoio para um `select`, não uma tela.
⛔ **Não** incluir `Ementa`, `CargaHoraria` nem o array `Docentes` — o último fecha ciclo de serialização, que é a mesma armadilha da `A/T1`.
⛔ **Não** criar `POST`, `PUT` nem `PATCH` de disciplina. Esta fatia **lê**.
⛔ **Não** filtrar disciplinas em silêncio (sem docente, inativas). Se você achar que um filtro é necessário, **reporte em CONTRADIÇÕES** em vez de aplicar — filtro que o servidor aplica calado produz uma tela que mente sobre o que existe.

**Critério de aceite:** no Swagger, o endpoint responde **200 com `application/json`** — não 302, que é o sintoma do `GlobalExceptionHandler` engolindo exceção. O array tem pelo menos duas disciplinas e cada item tem **exatamente dois campos**.

**Dependências:** nenhuma. **Rollback:** apagar o controller e o DTO; nada consome.

**Estimativa:** 3h

### T2 — `GET /api/docentes/{id}` com DTO de detalhe

**Contexto mínimo:** a `A/D-2` tirou `Cpf` e `DataNascimento` do DTO da listagem de propósito. O formulário de **edição** precisa da data de nascimento, e ela não vem na lista — daí o quinto endpoint (**D-1**). O `ObterPeloIdAsync` já existe no `IDocenteService`, sem rota.

**Arquivos-alvo:** `DocentesController.cs`; um `DocenteDetalheDTO` em `API/DTOs/DocenteDTOs/`.

**O que fazer:** `DocenteDetalheDTO` com exatamente `Id`, `Nome`, `Email`, `DataNascimento`, `DisciplinaId`, `Ativo`. Expor `GET /api/docentes/{id}` delegando ao `ObterPeloIdAsync`, devolvendo **404** para id inexistente.

⛔ **Não incluir `Cpf`** (**D-2**). O campo é imutável — `EditarDocenteDTO` não o tem — e a tela de edição não o exibe. Trazê-lo seria pôr dado pessoal no navegador sem que ninguém possa agir sobre ele.
⛔ **Não incluir `DisciplinaNome`**: o formulário pré-seleciona o `select` pelo **id**, e o nome já vem na listagem.
⛔ **Não** reusar o `DocenteListaDTO`. Ele tem cinco campos e um teste de reflexão que falha por acréscimo — mexer nele quebra a Fatia A.
⚠️ **Onde projetar:** a `A/§7` do contrato capturado registra que `Repository.csproj` **não referencia** `API.csproj`, então um `Select` para um tipo de `API/DTOs/` dentro do repositório fecharia ciclo de projetos. A projeção da Fatia A ficou no **controller** por isso. Siga o mesmo lugar, ou **reporte em CONTRADIÇÕES** se o grafo de dependências mudou.

**Critério de aceite:** no Swagger, `GET /api/docentes/{id}` de um docente existente responde **200** com **exatamente seis campos** e **sem `cpf`** (`CA-1`, `CA-2`). Um id inexistente responde **404**, não 302 nem 500 (`CA-3`). Colar as duas respostas na evidência.

**Dependências:** nenhuma. **Rollback:** remover a action e o DTO.

**Estimativa:** 3h

### T3 — Corrigir os dois crashes de `null` no cadastro

**Contexto mínimo:** hoje `dataNascimento: null` lança `InvalidOperationException` e `cpf: null` lança `NullReferenceException`. **Nenhum dos dois é `RegraDeNegocioException`**, então escapam para o `GlobalExceptionHandler`, que redireciona para uma rota inexistente — o cliente recebe **404 mudo** em vez de erro de validação. No MVC o `ModelState` protege; no REST não há nada. Decisão **D-6**.

Esta é a mesma situação da `A/D-5`: a fatia entrega justamente o endpoint que dispara a falha, então a correção entra aqui e não vira dívida. E o front nunca emitir `null` (D-3) **não é barreira** — Swagger, curl e qualquer outro cliente passam por fora dela.

**Arquivos-alvo:** o service/validação do cadastro de docente. ⚠️ Antes de editar, **leia onde o crash realmente acontece** — as duas exceções podem ter origens diferentes.

**O que fazer:** validar `Cpf` e `DataNascimento` nulos **antes** do ponto que estoura, e devolver **400** com mensagem. Usar o mesmo mecanismo que o restante da API já usa para 400 de validação (`RegraDeNegocioException` ou `BadRequest`, o que estiver estabelecido — **não inventar um terceiro**).

⛔ **Não corrigir o `GlobalExceptionHandler`.** Ele é infraestrutura da API inteira e está fora do escopo desta fatia. Esta task remove **duas causas conhecidas**, não o mecanismo.
⛔ **Não** acrescentar validação nova além dessas duas (formato de CPF, unicidade, idade mínima). Se você achar que alguma falta, **reporte em CONTRADIÇÕES**.

**Critério de aceite:** no Swagger, `POST /api/docentes` com `"cpf": null` responde **400** e com `"dataNascimento": null` responde **400** — nenhum dos dois responde 404, 500 ou 302 (`CA-4`). Colar as duas respostas literais, **incluindo o `Content-Type`**.

⚠️ **Evidência exigida nos dois sentidos:** rode os dois casos **antes** da correção e cole o que acontece hoje. Sem o "antes", o 400 do "depois" não prova que a correção fez diferença — pode ser que outra coisa já tratasse. Foi assim que a Fatia A soube que a correção do `InativarAsync` valia.

**Dependências:** T4 (o `POST` precisa existir para ser exercitado). ⚠️ **Ou** T4 primeiro, **ou** exercitar por teste de unidade — declare qual dos dois você fez.

**Rollback:** remover as guardas. **Estimativa:** 2h

### T4 — `POST /api/docentes`

**Contexto mínimo:** `AdicionarDocenteAsync` já existe no `IDocenteService`, sem rota. `DocentesController` publica quatro rotas depois da Fatia A e nenhuma de escrita.

**Arquivos-alvo:** `DocentesController.cs`; o `DocenteInputDTO` **existente** (ler antes de assumir a forma).

**O que fazer:** expor `POST /api/docentes` delegando ao `AdicionarDocenteAsync`, no mesmo padrão do `AlunosController`. Declarar `[ProducesResponseType]` para 201 (ou 200, o que o padrão do projeto usar) e 400.

⚠️ **Reportar, não decidir:** o front supõe que o corpo aceite `nome`, `cpf`, `email`, `dataNascimento` e `disciplinaId`. **Se o `DocenteInputDTO` não tiver `DisciplinaId`, NÃO acrescente** — reporte em CONTRADIÇÕES com a forma real do DTO. O vínculo com disciplina sai da fatia e vira fatia própria; é decisão do dono do front, não sua.

⛔ **Não tocar** no `GET /api/docentes` (13 arquivos do front dependem dele) nem no `POST /api/docentes/buscar` (entregue na Fatia A).
⛔ **Não** acrescentar campo ao `DocenteInputDTO` "para o front ter mais opções".

**Critério de aceite:** no Swagger, um `POST` válido cria o docente e ele **aparece** no `POST /api/docentes/buscar` (`CA-7`). Colar o corpo enviado e a resposta literal, incluindo o status e o `Location` se houver. Confirmar no banco que o `DisciplinaId` gravou o valor enviado — ou reportar que o campo não existe.

**Dependências:** T1 (para haver um `disciplinaId` real a enviar). **Rollback:** remover a action.

**Estimativa:** 3h

### T5 — `PUT /api/docentes/{id}`

**Contexto mínimo:** `EditarDocenteAsync` já existe sem rota. O `EditarDocenteDTO` **não tem `Cpf`** — é isso que torna o CPF imutável (**D-2**, **D-3**) e é o fato que molda o formulário do front.

**Arquivos-alvo:** `DocentesController.cs`; o `EditarDocenteDTO` **existente** (ler antes de assumir).

**O que fazer:** expor `PUT /api/docentes/{id}` delegando ao `EditarDocenteAsync`. **404** para id inexistente, **400** para validação.

⛔ **Não acrescentar `Cpf`** ao `EditarDocenteDTO`. A imutabilidade do CPF é contrato, e o front foi desenhado em cima dela.
⚠️ **Reportar:** se `DataNascimento` for obrigatória no `EditarDocenteDTO`, confirme — o front tornou o campo obrigatório nos dois modos (**D-3**) contando com isso. Se for **opcional**, diga: muda a leitura da decisão, embora não quebre nada.
⚠️ **Reportar:** se editar sem enviar `DisciplinaId` **apaga** o vínculo existente. Isso é a mesma classe de bug do `InativarAsync` da Fatia A, e o front precisa saber para sempre mandar o campo.

**Critério de aceite:** um `PUT` válido altera o docente, e o `GET /api/docentes/{id}` (T2) devolve o valor novo. Editar um docente **que tem disciplina** enviando o mesmo `disciplinaId` **preserva** o vínculo — conferir no banco que não ficou nulo. Colar corpo enviado e resposta.

**Dependências:** T2. **Rollback:** remover a action.

**Estimativa:** 3h

### T6 — Capturar o contrato real

**Contexto mínimo:** é a ponte entre os dois blocos. Interface TypeScript é promessa sobre JSON, não validação — e este projeto tem um campo fantasma vivo (`totalCriados` no front contra `totalCriado` no back) para provar o custo de supor. Na Fatia A esta task revelou `totalPaginas: 0` e o `400` em `text/plain`, dois fatos que viraram teste e que ninguém teria escrito de cabeça.

**Arquivos-alvo:** nenhum de código. O entregável é registro.

**O que fazer:** chamar os endpoints novos contra o banco real e **colar a resposta literal** — não a descrição dela — em `docs/desenvolvimento-com-ia/CONTRATO-DOCENTES-CADASTRO-CAPTURADO.md`, no formato do [CONTRATO-DOCENTES-CAPTURADO.md](CONTRATO-DOCENTES-CAPTURADO.md).

Casos obrigatórios:

1. `GET /api/disciplinas` com pelo menos duas disciplinas.
2. `GET /api/docentes/{id}` de um docente **com** disciplina.
3. `GET /api/docentes/{id}` de um docente **sem** disciplina — o contrato da Fatia A prova que existe (`"nome":"Arthur Professor","disciplinaNome":null`). É o caso que mostra o que `disciplinaId` vira quando não há vínculo: `null`, `0` ou campo ausente.
4. `GET /api/docentes/{id}` de id inexistente.
5. `POST` válido — corpo enviado **e** resposta.
6. `POST` com `cpf: null` e `POST` com `dataNascimento: null`, **antes e depois** da T3.
7. `POST` com CPF já cadastrado — para responder a pergunta aberta 2 da spec.
8. `PUT` válido — corpo enviado **e** resposta.

**Critério de aceite:** o arquivo existe e cada campo aparece com o **nome, o tipo e o casing exatos** que saíram do servidor, mais o `Content-Type` de cada resposta de erro. O formato literal de `dataNascimento` está registrado como o servidor o escreveu — com hora, se vier com hora.

**Dependências:** T1–T5. **Rollback:** não se aplica.

**Estimativa:** 2h

**Total do bloco 1: 16h.**

## Bloco 2 — Front-end — AINDA NÃO ESCRITO

Será escrito depois da T6, contra o contrato capturado. O corte previsto (do plano) é: contratos → disciplina service+facade → docente service → docente facade → smart+i18n → fechamento. **Escrever antes seria escrever o critério de aceite de "traduzir JSON em tipo" sobre um JSON que ainda não existe.**

## Apêndice — prompt autocontido para a sessão do back-end

> Para colar na sessão do repositório `DesafioTecnico1-Fundamentos`. O back **não tem** as skills, o harness nem nenhum destes documentos, então tudo que ele precisa saber está inline. Na Fatia A a seção de CONTRADIÇÕES pagou três vezes — uma delas revelou que a correção do `InativarAsync` mudou o comportamento observável de um endpoint que ninguém tocou.

```text
Você está no repositório back-end (.NET) do Gestão de Turmas. O front é outro
repositório e outra sessão; você não o acessa. Este trabalho é o bloco de
back-end de uma fatia vertical chamada "Docentes: cadastro e edição".

CONTEXTO QUE VOCÊ NÃO TEM COMO DESCOBRIR SOZINHO

O front já consome quatro rotas de docente, entregues numa fatia anterior:
  GET   /api/docentes                    lista achatada com disciplina (13
                                         arquivos do front dependem dela)
  POST  /api/docentes/buscar             busca paginada, DTO de 5 campos
  PATCH /api/docentes/{id}/inativar
  PATCH /api/docentes/{id}/reativar

Três decisões daquela fatia continuam valendo e restringem esta:
  - O DTO da listagem NÃO carrega Cpf nem DataNascimento, de propósito: dado
    que não trafega não vaza. Existe um teste de reflexão (DocenteListaDTOTests)
    que guarda os 5 campos e falha tanto por acréscimo quanto por remoção.
  - A projeção ficou no CONTROLLER, não no repositório, porque
    Repository.csproj não referencia API.csproj e um Select para um tipo de
    API/DTOs/ fecharia ciclo de projetos.
  - O GlobalExceptionHandler redireciona exceção não capturada para uma rota
    inexistente. Consequência: qualquer crash chega ao cliente como 404 mudo,
    sem relação com a causa. Corrigi-lo está FORA do escopo desta fatia.

O QUE ESTA FATIA PRECISA — cinco entregas

1. GET /api/disciplinas — controller REST novo sobre o IDisciplinaService que
   já existe. Array de objetos com exatamente Id e Nome. Sem paginação. Sem
   Ementa, sem CargaHoraria e SEM o array Docentes (esse fecha ciclo de
   serialização). Mesmos papéis do DocentesController.
   Sobe PRIMEIRO: é a única peça sem nada pronto por baixo, e o formulário do
   front precisa do id da disciplina. Sem ela o campo nasce contra um formato
   imaginado.

2. GET /api/docentes/{id} — fachada sobre ObterPeloIdAsync, que já existe sem
   rota. DTO de detalhe novo com exatamente:
       Id, Nome, Email, DataNascimento, DisciplinaId, Ativo
   NÃO incluir Cpf: o campo é imutável (EditarDocenteDTO não o tem), a tela de
   edição não o exibe, e trazê-lo seria pôr dado pessoal no navegador sem que
   ninguém possa agir sobre ele.
   NÃO incluir DisciplinaNome: o front pré-seleciona o campo pelo id.
   NÃO reusar o DocenteListaDTO — o teste de reflexão dele quebra.
   404 para id inexistente.

3. Corrigir dois crashes de null no cadastro. Hoje:
       dataNascimento: null  ->  InvalidOperationException
       cpf: null             ->  NullReferenceException
   Nenhum é RegraDeNegocioException, então os dois escapam para o
   GlobalExceptionHandler e chegam ao cliente como 404 mudo em vez de erro de
   validação. No MVC o ModelState protege; no REST não há nada.
   Os dois passam a responder 400, usando o mecanismo de 400 que a API JÁ usa —
   não inventa um terceiro.
   NÃO acrescente validação além dessas duas (formato de CPF, unicidade, idade).
   Se achar que falta alguma, reporte em CONTRADIÇÕES.

4. POST /api/docentes — fachada sobre AdicionarDocenteAsync, que já existe sem
   rota. Padrão do AlunosController.
   ⚠️ LEIA o DocenteInputDTO antes de assumir a forma dele. O front supõe que o
   corpo aceite nome, cpf, email, dataNascimento e disciplinaId. SE O DTO NÃO
   TIVER DisciplinaId, NÃO ACRESCENTE — reporte em CONTRADIÇÕES com a forma
   real. Nesse caso o vínculo com disciplina sai da fatia, e essa é decisão do
   dono do front, não sua.

5. PUT /api/docentes/{id} — fachada sobre EditarDocenteAsync, que já existe sem
   rota. 404 para id inexistente, 400 para validação.
   NÃO acrescente Cpf ao EditarDocenteDTO: a imutabilidade do CPF é contrato, e
   o formulário do front foi desenhado em cima dela.

O QUE NÃO FAZER
  - Não tocar no GET /api/docentes nem no POST /api/docentes/buscar.
  - Não corrigir o GlobalExceptionHandler.
  - Não criar POST, PUT ou PATCH de disciplina. Esta fatia LÊ disciplinas.
  - Não paginar /api/disciplinas.
  - Não filtrar disciplinas em silêncio (sem docente, inativas). Se achar que um
    filtro é necessário, reporte em CONTRADIÇÕES em vez de aplicar: filtro que o
    servidor aplica calado produz uma tela que mente sobre o que existe.
  - Não acrescentar campo a DTO nenhum "para o front ter mais opções".

COMO EU QUERO O RELATÓRIO — evidência, não afirmação

"Implementei o endpoint" não é aceite. Para CADA entrega, cole:
  - a requisição literal (verbo, URL, corpo);
  - a resposta literal (status, Content-Type e corpo, sem reformatar);
  - o que você conferiu NO BANCO, quando a entrega grava algo.

Casos de erro são obrigatórios, não opcionais:
  - GET /api/docentes/{id} de id inexistente;
  - POST com cpf: null e POST com dataNascimento: null — ANTES e DEPOIS da
    correção. Sem o "antes", o 400 do "depois" não prova que sua correção fez
    diferença: pode ser que outra coisa já tratasse o caso;
  - POST com CPF já cadastrado — quero saber o status, seja ele qual for;
  - GET /api/docentes/{id} de um docente SEM disciplina. Sei que existe pelo
    menos um. É o caso que mostra o que DisciplinaId vira quando não há
    vínculo: null, 0 ou campo ausente. Os três dão código diferente no front.

E registre o formato LITERAL de DataNascimento como o servidor escreveu — com
hora, se vier com hora. Não normalize, não formate, não "limpe".

SEÇÃO OBRIGATÓRIA: CONTRADIÇÕES

Termine o relatório com uma seção "CONTRADIÇÕES", e ela não pode ficar vazia por
omissão — se não houver nenhuma, escreva "nenhuma" e diga o que você conferiu
para poder afirmar isso.

Liste tudo em que a realidade do código divergiu deste prompt:
  - decisão minha que não é implementável como escrita, e por quê;
  - campo, método ou classe que eu afirmei existir e não existe (ou tem outro
    nome, outro tipo, outro casing);
  - lugar onde o padrão que eu mandei seguir não é o padrão real do projeto;
  - qualquer entrega desta lista que mudou o comportamento observável de um
    endpoint que EU NÃO PEDI para mexer. Isto é o item mais importante da
    seção. Na fatia anterior, uma correção de bug num método de repositório
    mudou o resultado do GET /api/docentes, que ninguém tocou, e só apareceu
    porque esta seção era obrigatória.

Duas perguntas específicas que eu quero respondidas nominalmente:
  1. O EditarDocenteDTO exige DataNascimento? O front tornou o campo
     obrigatório nos dois modos contando com isso.
  2. Editar sem enviar DisciplinaId APAGA o vínculo existente? Se apagar, o
     front precisa saber para sempre mandar o campo. Confira no banco, não no
     código.

Se em qualquer momento você concluir que uma instrução minha está errada,
PARE e diga — não contorne em silêncio e não implemente os dois caminhos.
```
