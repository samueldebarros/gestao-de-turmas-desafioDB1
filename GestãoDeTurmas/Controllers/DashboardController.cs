using API.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Coordenador")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly string mensagemStatus500 = "Ocorreu um erro ao processar a requisição";

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("painel-demografico")]
    public async Task<IActionResult> ObterPainelDemografico()
    {
        try
        {
            var dados = await _dashboardService.ObterPainelDemograficoPorTurmaAsync();
            return Ok(dados);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }

    [HttpGet("balanco-evasao")]
    public async Task<IActionResult> ObterBalancoEvasao()
    {
        try
        {
            var dados = await _dashboardService.ObterBalancoEvasaoPorSerieAsync();
            return Ok(dados);
        }
        catch (Exception)
        {
            return StatusCode(500, mensagemStatus500);
        }
    }
}
