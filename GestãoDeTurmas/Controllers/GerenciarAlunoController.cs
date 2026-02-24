using API.DTOs;
using API.Service;
using Common.Domains;
using Common.Enums;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

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
        public async Task<IActionResult> Adicionar(AlunoInputViewModel model)
        {
            if (!ModelState.IsValid) {
                return View(model);
            }
            try
            {
                var alunoDto = model.ToDTO();

                await _alunoService.AdicionarAlunoAsync(alunoDto);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var mensagemErro = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                ModelState.AddModelError(string.Empty, mensagemErro);
                return View("Adicionar", model);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            await _alunoService.ExcluirAlunoAsync(id);
            return RedirectToAction("Index");
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
        public async Task<IActionResult> Editar(AlunoEditarViewModel model)
        {
            if (!ModelState.IsValid) return View("Editar", model);
            var alunoAlterado = model.ToAlterarDTO();
            try
            {
                await _alunoService.AlterarAsync(alunoAlterado);
                return RedirectToAction("Index");
            } catch (Exception ex)
            {
                var mensagemErro = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                ModelState.AddModelError(string.Empty, mensagemErro);
                return View(model);
            }

        }
    }
}
