using API.DTOs.DisciplinaDTOs;
using API.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Coordenador")]
public class DisciplinasController : ControllerBase
{
    private readonly IDisciplinaService _disciplinaService;
    private readonly string mensagemStatus500 = "Ocorreu um erro ao processar a requisição";

    public DisciplinasController(IDisciplinaService disciplinaService)
    {
        _disciplinaService = disciplinaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DisciplinaListaDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterDisciplinas()
    {
        try
        {
            // Sem paginação e sem filtro: o único método de serviço que devolve TODAS as
            // disciplinas é paginado, então pedimos uma página só. ObterDisciplinasAtivasAsync
            // existe, mas filtraria inativas em silêncio — e a inativa precisa chegar ao front
            // para que o formulário de edição consiga exibir o vínculo vigente de um docente.
            // Quem esconde a inativa do select é o front, pelo campo Ativo.
            var lista = await _disciplinaService.ObterTodasAsDisciplinasAsync(1, int.MaxValue);

            var disciplinas = lista
                .Select(d => new DisciplinaListaDTO(d.Id, d.Nome, d.Ativo))
                .ToList();

            return Ok(disciplinas);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }
}
