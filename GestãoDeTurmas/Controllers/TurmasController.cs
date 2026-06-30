using API.DTOs.TurmaDTOs;
using API.Service;
using Common.Enums;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Coordenador")]
public class TurmasController : ControllerBase
{
    private readonly ITurmaService _turmaService;
    private readonly string mensagemStatus500 = "Ocorreu um erro ao processar a requisição";

    public TurmasController(ITurmaService turmaService)
    {
        _turmaService = turmaService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTurmas(
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 12,
        [FromQuery] string? pesquisa = null, [FromQuery] int? anoLetivo = null,
        [FromQuery] TurnoEnum? turno = null, [FromQuery] bool? ativo = null,
        [FromQuery] OrdenacaoTurmaEnum? ordenacao = null)
    {
        if (ordenacao.HasValue && !Enum.IsDefined(ordenacao.Value))
            return BadRequest("Ordenação inválida");

        try
        {
            var lista = await _turmaService.ObterTurmasAsync(
                pagina, tamanhoPagina, pesquisa, anoLetivo, turno, ativo, ordenacao);
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
   
