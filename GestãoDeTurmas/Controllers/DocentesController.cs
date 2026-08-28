using API.DTOs.DocenteDTOs;
using API.Service;
using Common.Enums;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Coordenador")]
    public class DocentesController : ControllerBase
    {
        private readonly IDocenteService _docenteService;
        private readonly string mensagemStatus500 = "Ocorreu um erro ao processar a requisição";

        public DocentesController(IDocenteService docenteService) {
            _docenteService = docenteService;
        }

        private static string? MapearOrdenacao(OrdenacaoDocenteEnum? ordenacao) => ordenacao switch
        {
            OrdenacaoDocenteEnum.Nome => "Nome",
            OrdenacaoDocenteEnum.Disciplina => "Disciplina",
            _ => null
        };

        [HttpGet]
        public async Task<IActionResult> ObterDocentesDisciplinasAsync()
        {
            try
            {
                var listaDocentesComDiscplinas = await _docenteService.ObterDocentesDisciplinasSqlAsync();
                return Ok(listaDocentesComDiscplinas);
            }
            catch (Exception)
            {
                return StatusCode(500, mensagemStatus500);
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DocenteDetalheDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterDocentePorId(int id)
        {
            try
            {
                var docente = await _docenteService.ObterPeloIdAsync(id);
                if (docente is null) return NotFound();

                return Ok(new DocenteDetalheDTO(
                    docente.Id, docente.Nome, docente.Email,
                    docente.DataNascimento, docente.DisciplinaId, docente.Ativo));
            }
            catch (Exception)
            {
                return StatusCode(500, mensagemStatus500);
            }
        }

        [HttpPost("buscar")]
        [ProducesResponseType(typeof(DocenteBuscaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarDocentes([FromBody] DocenteBuscaRequest request)
        {
            if (request.Ordenacao.HasValue && !Enum.IsDefined(request.Ordenacao.Value))
                return BadRequest("Ordenação Inválida");
            if (request.Direcao.HasValue && !Enum.IsDefined(request.Direcao.Value))
                return BadRequest("Direção Inválida");

            try
            {
                var lista = await _docenteService.ObterTodosOsDocentesAsync(
                    request.Pagina, request.TamanhoPagina, request.Pesquisa,
                    request.Ativo, MapearOrdenacao(request.Ordenacao), request.Direcao);

                var itens = lista
                    .Select(d => new DocenteListaDTO(d.Id, d.Nome, d.Email, d.Disciplina?.Nome, d.Ativo))
                    .ToList();

                return Ok(new
                {
                    itens,
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarDocente([FromBody] DocenteInputDTO novoDocente)
        {
            try
            {
                await _docenteService.AdicionarDocenteAsync(novoDocente);
                return StatusCode(StatusCodes.Status201Created);
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
        public async Task<IActionResult> EditarDocente(int id, [FromBody] EditarDocenteDTO docente)
        {
            docente.Id = id;

            try
            {
                await _docenteService.EditarDocenteAsync(docente);
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
        public async Task<IActionResult> InativarDocente(int id)
        {
            try
            {
                await _docenteService.InativarDocenteAsync(id);
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReativarDocente(int id)
        {
            try
            {
                await _docenteService.ReativarDocenteAsync(id);
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
}
