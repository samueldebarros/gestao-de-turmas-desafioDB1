using API.DTOs.AlunoDTOs;
using API.Exceptions;
using API.Service;
using Common;
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
            var aluno = await _alunoService.AdicionarAlunoAsync(novoAluno.ToDTO());
            var dto = new AlunoCriadoDTO(aluno.Id, aluno.Matricula, aluno.Cpf);
            return Created($"/api/alunos/{aluno.Id}", dto);
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

    [HttpPost("importar")]
    [Authorize(Roles = "Admin,Coordenador")]
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
            return Created($"/api/alunos", resultado);
        }
        catch (ImportacaoInvalidaException ex)
        {
            return UnprocessableEntity(new ImportacaoErroDTO(ex.Erros));
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }
}
