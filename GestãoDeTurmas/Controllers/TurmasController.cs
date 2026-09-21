using API.DTOs.TurmaDTOs;
using API.Service;
using Common.Enums;
using Common.Exceptions;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Repositories;

namespace GestãoDeTurmas.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Coordenador")]
public class TurmasController : ControllerBase
{
    private readonly ITurmaService _turmaService;
    private readonly string mensagemStatus500 = "Ocorreu um erro ao processar a requisição";
    private const int TamanhoMaximoComInclusao = 100;

    public TurmasController(ITurmaService turmaService)
    {
        _turmaService = turmaService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTurmas(
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 12,
        [FromQuery] string? pesquisa = null, [FromQuery] int? anoLetivo = null,
        [FromQuery] TurnoEnum? turno = null, [FromQuery] bool? ativo = null,
        [FromQuery] OrdenacaoTurmaEnum? ordenacao = null, [FromQuery] string? incluir = null)
    {
        if (ordenacao.HasValue && !Enum.IsDefined(ordenacao.Value))
            return BadRequest("Ordenação inválida");

        if (!InclusaoTurma.TentarInterpretar(incluir, out var inclusao))
            return BadRequest("Valor inválido em 'incluir'. Aceitos: docentes, alunos.");

        if (inclusao != InclusaoTurmaEnum.Nenhum && tamanhoPagina > TamanhoMaximoComInclusao)
            return BadRequest($"'incluir' exige tamanhoPagina de no máximo {TamanhoMaximoComInclusao}.");

        try
        {
            var lista = await _turmaService.ObterTurmasAsync(
                pagina, tamanhoPagina, pesquisa, anoLetivo, turno, ativo, ordenacao, inclusao);
            return Ok(new
            {
                itens = lista,
                lista.PaginaAtual,
                lista.TotalPaginas,
                lista.TotalResultados,
                lista.TamanhoPagina
            });
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpGet("{id:int}/docentes")]
    [ProducesResponseType(typeof(List<DocenteSqlDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterDocentesDaTurma(int id)
    {
        try
        {
            var docentes = await _turmaService.ObterDocentesDaTurmaAsync(id);
            return Ok(docentes);
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpGet("{id:int}/alunos")]
    [ProducesResponseType(typeof(List<AlunoDaTurmaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterAlunosDaTurma(int id)
    {
        try
        {
            var alunos = await _turmaService.ObterAlunosDaTurmaAsync(id);
            return Ok(alunos);
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpGet("{id:int}/alunos-disponiveis")]
    [ProducesResponseType(typeof(List<AlunoResumoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterAlunosDisponiveis(int id)
    {
        try
        {
            var alunos = await _turmaService.ObterAlunosDisponiveisAsync(id);
            return Ok(alunos);
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AdicionarTurma([FromBody] TurmaInputDTO novaTurma)
    {
        try
        {
            await _turmaService.AdicionarTurmaAsync(novaTurma);
            return Ok();
        }
        catch (RegraDeNegocioException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> EditarTurma(int id, [FromBody] TurmaEditarDTO turma)
    {
        turma.Id = id;

        try
        {
            await _turmaService.EditarTurmaAsync(turma);
            return NoContent();
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (RegraDeNegocioException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpPost("{id:int}/alunos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> MatricularAluno(int id, [FromBody] MatricularAlunoDTO matricula)
    {
        try
        {
            await _turmaService.MatricularAlunoAsync(id, matricula.AlunoId);
            return NoContent();
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (RegraDeNegocioException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpPatch("{id:int}/alunos/{alunoId:int}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelarMatricula(int id, int alunoId)
    {
        try
        {
            await _turmaService.CancelarMatriculaAsync(id, alunoId);
            return NoContent();
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (RegraDeNegocioException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpPost("{id:int}/docentes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> VincularDocente(int id, [FromBody] VincularDocenteDTO alocacao)
    {
        try
        {
            await _turmaService.VincularDocenteAsync(id, alocacao.DocenteId);
            return NoContent();
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (RegraDeNegocioException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpDelete("{id:int}/disciplinas/{disciplinaId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DesvincularDisciplina(int id, int disciplinaId)
    {
        try
        {
            await _turmaService.DesvincularDisciplinaAsync(id, disciplinaId);
            return NoContent();
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (RegraDeNegocioException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpPatch("{id}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> InativarTurma(int id)
    {
        try
        {
            await _turmaService.InativarTurmaAsync(id);
            return NoContent();
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (RegraDeNegocioException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpPatch("{id}/reativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReativarTurma(int id)
    {
        try
        {
            await _turmaService.ReativarTurmaAsync(id);
            return NoContent();
        }
        catch (EntidadeNaoEncontradaException)
        {
            return NotFound();
        }
        catch (RegraDeNegocioException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

}
   
