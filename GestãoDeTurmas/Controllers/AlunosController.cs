using API.Service;
using Common.Enums;
using Common.Exceptions;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Aluno;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles ="Admin,Coordenador,Docente")]
public class AlunosController : ControllerBase
{
    private readonly IAlunoService _alunoService;
    private readonly string mensagemStatus500 = "Ocorreu um erro ao processar a requisição";
    public AlunosController(IAlunoService alunoService)
    {
        _alunoService = alunoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodosOsAlunos([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 10, [FromQuery] string? pesquisa = null, [FromQuery] SexoEnum? sexo = null,
        [FromQuery] bool? ativo = null, [FromQuery] OrdenacaoAlunoEnum? ordenacao = null, [FromQuery] DirecaoOrdenacaoEnum? direcao = null)
    {
        if (ordenacao.HasValue && !Enum.IsDefined(ordenacao.Value))
            return BadRequest("Ordenação inválida");
        if (direcao.HasValue && !Enum.IsDefined(direcao.Value))
            return BadRequest("Direção inválida");

        try
        {
            var lista = await _alunoService.ObterTodosOsAlunosAsync(pagina, tamanhoPagina, pesquisa, sexo, ativo, ordenacao, direcao);
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
    public async Task<IActionResult> AdicionarAluno([FromBody] AlunoInputViewModel novoAluno)
    {
        try
        {
            var alunoDTO = novoAluno.ToDTO();
            await _alunoService.AdicionarAlunoAsync(alunoDTO);
            return Ok();
        } catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }

    }

    [HttpPatch("{id}/inativar")]
    public async Task<IActionResult> InativarAluno(int id)
    {
        try
        {
            await _alunoService.InativarAlunoAsync(id);
            return NoContent();
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

    [HttpPatch("{id}/reativar")]
    public async Task<IActionResult> ReativarAluno(int id)
    {
        try
        {
            await _alunoService.ReativarAlunoAsync(id);
            return NoContent();
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

    [HttpPut("{id}")]
    public async Task<IActionResult> EditarAluno(int id, [FromBody] AlunoEditarViewModel aluno)
    {
        try
        {
            var alunoDTO = aluno.ToEditarDTO();
            await _alunoService.EditarAlunoAsync(alunoDTO);
            return NoContent();
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
}
