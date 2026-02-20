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
        public IActionResult Index(int pagina = 1, string? pesquisa = null, SexoEnum? sexo = null, bool? ativo = null)
        {
            ViewBag.PesquisaAtual = pesquisa;
            ViewBag.SexoAtual = sexo;
            ViewBag.AtivoAtual = ativo;

            var alunosPaginados = _alunoService.ObterTodosOsAlunos(pagina, TAMANHO_PAGINA, pesquisa, sexo, ativo);

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

        public IActionResult Adicionar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(GerenciarAlunoViewModel model)
        {
            if (!ModelState.IsValid) {
                return View("Adicionar", model);
            }
            try
            {
                var alunoDto = model.NovoAluno.ToDTO();
                _alunoService.AdicionarAluno(alunoDto);

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
        public IActionResult Excluir(int id)
        {
            _alunoService.ExcluirAluno(id);
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            var aluno = _alunoService.ObterPeloId(id);
            if (aluno == null) return NotFound();

            AlunoEditarViewModel alunoEditarViewModel = aluno.ToEditarViewModel();
            
            return View(alunoEditarViewModel);
        }

        [HttpPost]
        public IActionResult Alterar(AlunoEditarViewModel model)
        {
            if (!ModelState.IsValid) return View("Editar", model);
            var alunoAlterado = model.ToAlterarDTO();
            try
            {
                _alunoService.Alterar(alunoAlterado);
                return RedirectToAction("Index");
            } catch (Exception ex)
            {
                var mensagemErro = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                ModelState.AddModelError(string.Empty, mensagemErro);
                return View("Editar", model);
            }

        }
    }
}
