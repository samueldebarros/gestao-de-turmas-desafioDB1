using API.Service;
using Common.Exceptions;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Docente;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers
{
    public class GerenciarDocenteController : Controller
    {
        private readonly IDocenteService _docenteService;
        public GerenciarDocenteController(IDocenteService docenteService)
        {
            _docenteService = docenteService;
        }

        public async Task<IActionResult> Index()
        {
            var docentes = await _docenteService.ObterTodosOsDocentesAsync();

            var docenteDomain = docentes.Select(a => a.ToListaViewModel()).ToList();

            var viewModel = new GerenciarDocenteViewModel()
            {
                DocentesCadastrados = docenteDomain,
                NovoDocente = new DocenteInputViewModel()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Adicionar()
        {
            return View(new DocenteInputViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Adicionar(DocenteInputViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            try
            {
                var docenteDTO = model.ToDTO();

                await _docenteService.AdicionarDocenteAsync(docenteDTO);

                return RedirectToAction(nameof(Index));
            }
            catch (RegraDeNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(nameof(Adicionar), model);
            }
        }
    }
}
