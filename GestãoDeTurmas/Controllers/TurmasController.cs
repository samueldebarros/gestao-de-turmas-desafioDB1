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

    [HttpPost]
    public async Task<IActionResult> AdicionarTurma([FromBody] TurmaInputDTO novaTurma)
    {
        try
        {
            await _turmaService.AdicionarTurmaAsync(novaTurma);
            return Ok();
        }
        catch (RegraDeNegocioException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

}
   
