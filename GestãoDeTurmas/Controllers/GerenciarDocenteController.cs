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
        private const int TAMANHO_PAGINA = 5;
        public GerenciarDocenteController(IDocenteService docenteService)
        {
            _docenteService = docenteService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pagina = 1,string? pesquisa = null, bool? ativo = null)
        {
            ViewBag.PesquisaAtual = pesquisa;
            ViewBag.AtivoAtual = ativo;

            var docentesPaginados = await _docenteService.ObterTodosOsDocentesAsync(pagina, TAMANHO_PAGINA,pesquisa,ativo);

            if (pagina > docentesPaginados.TotalPaginas && docentesPaginados.TotalPaginas > 0)
                return RedirectToAction(nameof(Index), new { pagina = docentesPaginados.TotalPaginas, pesquisa, ativo });

            var docenteDomain = docentesPaginados.Select(a => a.ToListaViewModel()).ToList();

            var viewModel = new GerenciarDocenteViewModel()
            {
                DocentesCadastrados = docenteDomain,
                NovoDocente = new DocenteInputViewModel(),

                PaginaAtual = pagina,
                TotalPaginas = docentesPaginados.TotalPaginas,
                TemProximaPagina = docentesPaginados.TemProximaPagina,
                TemPaginaAnterior = docentesPaginados.TemPaginaAnterior
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Adicionar()
        {
            return View(new DocenteInputViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inativar(int id, string? pesquisa, bool? ativo, int pagina = 1)
        {
            try
            {
                await _docenteService.InativarDocenteAsync(id);
                return RedirectToAction(nameof(Index), new {pagina, pesquisa, ativo});
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction(nameof(Index), new {pagina, pesquisa, ativo });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reativar(int id, string? pesquisa, bool? ativo, int pagina = 1)
        {
            try
            {
                await _docenteService.ReativarDocenteAsync(id);
                return RedirectToAction(nameof(Index), new { pagina, pesquisa, ativo });
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction(nameof(Index), new { pagina, pesquisa, ativo });
            }
        }
    }
}
