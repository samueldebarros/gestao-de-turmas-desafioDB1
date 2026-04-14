using API.Service;
using Microsoft.AspNetCore.Mvc;
using GestãoDeTurmas.Mappers;

namespace GestãoDeTurmas.Controllers
{
    public class PaginaInicialController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public PaginaInicialController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var DadosDTO = await _dashboardService.ObterDadosDashboard();

            var model = DadosDTO.ToViewModel();

            return View(model);
        }
    }
}
