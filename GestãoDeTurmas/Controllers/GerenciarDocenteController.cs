using API.Service;
using Common;
using Common.Enums;
using Common.Exceptions;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Docente;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestãoDeTurmas.Controllers
{
    public class GerenciarDocenteController : Controller
    {
        private readonly IDocenteService _docenteService;
        private readonly IDisciplinaService _disciplinaService;
        public GerenciarDocenteController(IDocenteService docenteService, IDisciplinaService disciplinaService)
        {
            _docenteService = docenteService;
            _disciplinaService = disciplinaService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pagina = 1, string? pesquisa = null, bool? ativo = null, string? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null)
        {
            var docentesPaginados = await _docenteService.ObterTodosOsDocentesAsync(pagina, Constantes.TAMANHO_PAGINA, pesquisa, ativo, ordenacao, direcao);

            if (pagina > docentesPaginados.TotalPaginas && docentesPaginados.TotalPaginas > 0)
                return RedirectToAction(nameof(Index), new { pagina = docentesPaginados.TotalPaginas, pesquisa, ativo });

            var docenteDomain = docentesPaginados.Select(a => a.ToListaViewModel()).ToList();

            var viewModel = new GerenciarDocenteViewModel()
            {
                DocentesCadastrados = docenteDomain,
                NovoDocente = new DocenteInputViewModel(),

                PaginaAtual = pagina,
                TotalPaginas = docentesPaginados.TotalPaginas,
                TotalResultados = docentesPaginados.TotalResultados,
                TemProximaPagina = docentesPaginados.TemProximaPagina,
                TamanhoPagina = docentesPaginados.TamanhoPagina,
                TemPaginaAnterior = docentesPaginados.TemPaginaAnterior,
                PesquisaAtual = pesquisa,
                FiltrosAtivos = new Dictionary<string, string>(),
                Ordenacao = ordenacao,
                Direcao = direcao,
                AtivoAtual = ativo,
            };

            if (!string.IsNullOrEmpty(pesquisa)) viewModel.FiltrosAtivos.Add("pesquisa", pesquisa);
            if (ativo != null) viewModel.FiltrosAtivos.Add("ativo", ativo.Value.ToString().ToLower());

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Adicionar()
        {
            await PopularDisciplinasAsync();
            return PartialView("_Adicionar", new DocenteInputViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(DocenteInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                await PopularDisciplinasAsync();
                return PartialView("_Adicionar", model);
            }

            try
            {
                var docenteDTO = model.ToDTO();

                await _docenteService.AdicionarDocenteAsync(docenteDTO);
                TempData["MensagemSucesso"] = "Docente adicionado com sucesso!";
                return Json(new { sucesso = true });
            }
            catch (RegraDeNegocioException ex)
            {
                Response.StatusCode = 400;
                await PopularDisciplinasAsync();
                ModelState.AddModelError(string.Empty, ex.Message);
                return PartialView("_Adicionar", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var docente = await _docenteService.ObterPeloIdAsync(id);
            if (docente == null) return NotFound();

            DocenteEditarViewModel viewModel = docente.ToEditarViewModel();

            await PopularDisciplinasAsync();
            return PartialView("_Editar", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(DocenteEditarViewModel viewModel)
        {

            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                await PopularDisciplinasAsync();
                return PartialView("_Editar", viewModel);
            }

            var docenteAlterado = viewModel.ToEditarDTO();
            try
            {
                await _docenteService.EditarDocenteAsync(docenteAlterado);
                TempData["MensagemSucesso"] = "Docente editado com sucesso!";
                return Json(new { sucesso = true });
            }
            catch (RegraDeNegocioException ex)
            {
                Response.StatusCode = 400;
                await PopularDisciplinasAsync();
                ModelState.AddModelError(string.Empty, ex.Message);
                return PartialView("_Editar", viewModel);
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                Response.StatusCode = 404;
                return Json(new { sucesso = false, mensagem = ex.Message });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inativar(int id, string? pesquisa, bool? ativo, int pagina = 1)
        {
            try
            {
                await _docenteService.InativarDocenteAsync(id);
                TempData["MensagemSucesso"] = "Docente inativado com sucesso!";
                return Ok();
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reativar(int id, string? pesquisa, bool? ativo, int pagina = 1)
        {
            try
            {
                await _docenteService.ReativarDocenteAsync(id);
                TempData["MensagemSucesso"] = "Docente reativado com sucesso!";
                return Ok();
            }
            catch (EntidadeNaoEncontradaException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        private async Task PopularDisciplinasAsync()
        {
            var disciplinas = await _disciplinaService.ObterDisciplinasAtivasAsync();
            ViewBag.Disciplinas = new SelectList(disciplinas, "Id", "Nome");

        }
    }
}
