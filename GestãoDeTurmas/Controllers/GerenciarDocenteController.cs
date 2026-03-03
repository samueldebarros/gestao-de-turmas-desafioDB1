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

        public async Task<IActionResult> Index(string? pesquisa, bool? ativo)
        {
            ViewBag.PesquisaAtual = pesquisa;
            ViewBag.AtivoAtual = ativo;

            var docentes = await _docenteService.ObterTodosOsDocentesAsync(pesquisa,ativo);

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

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var docente = await _docenteService.ObterPeloIdAsync(id);
            if (docente == null) return NotFound();

            DocenteEditarViewModel viewModel = docente.ToEditarViewModel();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(DocenteEditarViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(nameof(Editar),viewModel);

            var docenteAlterado = viewModel.ToEditarDTO();
            try
            {
                await _docenteService.EditarDocenteAsync(docenteAlterado);
                return RedirectToAction(nameof(Index));
            } 
            catch (RegraDeNegocioException ex)
            {
                ModelState.AddModelError(string.Empty,ex.Message);
                return View(viewModel);
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }

        }

        [HttpPost]
        public async Task<IActionResult> Inativar(int id)
        {
            try
            {
                await _docenteService.InativarDocenteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }

        }

        [HttpPost]
        public async Task<IActionResult> Reativar(int id)
        {
            try
            {
                await _docenteService.ReativarDocenteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
