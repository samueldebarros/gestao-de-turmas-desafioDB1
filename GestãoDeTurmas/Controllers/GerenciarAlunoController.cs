using API.Service;
using Common.Enums;
using Common.Exceptions;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Aluno;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers
{
    public class GerenciarAlunoController : Controller
    {
        private readonly IAlunoService _alunoService;
        private const int TAMANHO_PAGINA = 10;

        public GerenciarAlunoController(IAlunoService alunoService)
        {
            _alunoService = alunoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pagina = 1, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null)
        {
            ViewBag.PesquisaAtual = pesquisa;
            ViewBag.SexoAtual = sexo;
            ViewBag.AtivoAtual = ativo;

            var alunosPaginados = await _alunoService.ObterTodosOsAlunosAsync(pagina, TAMANHO_PAGINA, pesquisa, sexo, ativo);

            if(pagina > alunosPaginados.TotalPaginas && alunosPaginados.TotalPaginas > 0)
                return RedirectToAction(nameof(Index), new {pagina = alunosPaginados.TotalPaginas, pesquisa, sexo, ativo});

            var alunosDomain = alunosPaginados.Select(a => a.ToListaViewModel()).ToList();

            var viewModel = new GerenciarAlunoViewModel
            {
                AlunosCadastrados = alunosDomain,

                PaginaAtual = alunosPaginados.PaginaAtual,
                TotalPaginas = alunosPaginados.TotalPaginas,
                TemPaginaAnterior = alunosPaginados.TemPaginaAnterior,
                TemProximaPagina = alunosPaginados.TemProximaPagina,

                NovoAluno = new AlunoInputViewModel()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Adicionar()
        {
            return View(new AlunoInputViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(AlunoInputViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            try
            {
                var alunoDto = model.ToDTO();

                await _alunoService.AdicionarAlunoAsync(alunoDto);

                return RedirectToAction(nameof(Index));
            }
            catch (RegraDeNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(nameof(Adicionar), model);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inativar(int id, string? pesquisa, SexoEnum? sexo, bool? ativo, int pagina = 1)
        {
            await _alunoService.InativarAlunoAsync(id);
            return RedirectToAction(nameof(Index), new {pagina, pesquisa, sexo, ativo});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reativar(int id, string? pesquisa, SexoEnum? sexo, bool? ativo, int pagina = 1)
        {
            await _alunoService.ReativarAlunoAsync(id);
            return RedirectToAction(nameof(Index), new {pagina, pesquisa, sexo, ativo});
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var aluno = await _alunoService.ObterPeloIdAsync(id);
            if (aluno == null) return NotFound();

            AlunoEditarViewModel alunoEditarViewModel = aluno.ToEditarViewModel();
            
            return View(alunoEditarViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(AlunoEditarViewModel model)
        {
            if (!ModelState.IsValid) return View(nameof(Editar), model);

            var alunoAlterado = model.ToAlterarDTO();

            try
            {
                await _alunoService.AlterarAsync(alunoAlterado);
                return RedirectToAction(nameof(Index));
            } catch (RegraDeNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

        }
    }
}
