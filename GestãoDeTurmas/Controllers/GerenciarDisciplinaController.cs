using API.Service;
using Common;
using Common.Exceptions;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Disciplina;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestãoDeTurmas.Controllers;

public class GerenciarDisciplinaController : Controller
{
    private readonly IDisciplinaService _disciplinaService;

    public GerenciarDisciplinaController(IDisciplinaService disciplinaService)
    {
        _disciplinaService = disciplinaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1, string? pesquisa = null, bool? ativo = null)
    {
        ViewBag.PesquisaAtual = pesquisa;
        ViewBag.AtivoAtual = ativo;

        var listaDisciplinas = await _disciplinaService.ObterTodasAsDisciplinasAsync(pagina, Constantes.TAMANHO_PAGINA, pesquisa, ativo);

        if (pagina > listaDisciplinas.TotalPaginas && listaDisciplinas.TotalPaginas > 0)
            return RedirectToAction(nameof(Index), new { pagina = listaDisciplinas.TotalPaginas, pesquisa, ativo });

        var disciplinasDomain = listaDisciplinas.Select(d => d.ToListaViewModel()).ToList();

        var viewModel = new GerenciarDisciplinaViewModel()
        {
            DisciplinasCadastradas = disciplinasDomain,
            NovaDisciplina = new DisciplinaInputViewModel(),

            TemProximaPagina = listaDisciplinas.TemProximaPagina,
            TemPaginaAnterior = listaDisciplinas.TemPaginaAnterior,
            TamanhoPagina = listaDisciplinas.TamanhoPagina,
            TotalResultados = listaDisciplinas.TotalResultados,
            PaginaAtual = pagina,
            TotalPaginas = listaDisciplinas.TotalPaginas,
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Adicionar()
    {
        return PartialView("_Adicionar",new DisciplinaInputViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adicionar(DisciplinaInputViewModel model)
    {
        if (!ModelState.IsValid) return PartialView("_Adicionar",model);

        var disciplina = model.ToInputDTO();
        try
        {
            await _disciplinaService.AdicionarDisciplinaAsync(disciplina);
            TempData["MensagemSucesso"] = "Disciplina adicionada com sucesso!";
            return Json(new { sucesso = true });
        }
        catch(RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return PartialView("_Adicionar",model);

        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Já existe uma disciplina com este nome.");
            return View("_Adicionar",model);
        }

    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var disciplina = await _disciplinaService.ObterDisciplinaPorIdAsync(id);
        if (disciplina == null) return NotFound();

        var viewModel = disciplina.ToEditarViewModel();

        return PartialView("_Editar",viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(DisciplinaEditarViewModel viewModel)
    {
        if (!ModelState.IsValid) return PartialView("_Editar", viewModel);

        var disciplina = viewModel.ToEditarDTO();
        try
        {
            await _disciplinaService.EditarDisciplinaAsync(disciplina);

            TempData["MensagemSucesso"] = "Disciplina editada com sucesso!";
            return Json(new { sucesso = true });
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return PartialView("_Editar",viewModel);
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inativar(int id, string? pesquisa = null, bool? ativo = null, int pagina = 1)
    {
        try
        {
            await _disciplinaService.InativarDisciplinaAsync(id);
            TempData["MensagemSucesso"] = "Disciplina inativada com sucesso!";
            return RedirectToAction(nameof(Index), new { pesquisa, ativo, pagina});
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            TempData["MensagemErro"] = ex.Message;
            return RedirectToAction(nameof(Index), new { pesquisa, ativo, pagina });
        }
        catch (RegraDeNegocioException ex)
        {
            TempData["MensagemErro"] = ex.Message;
            return RedirectToAction(nameof(Index), new {pesquisa, ativo, pagina});
        }

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id, string? pesquisa = null, bool? ativo = null, int pagina = 1)
    {
        try
        {
            await _disciplinaService.ReativarDisciplinaAsync(id);
            TempData["MensagemSucesso"] = "Disciplina reativada com sucesso!";
            return RedirectToAction(nameof(Index), new { pesquisa, ativo, pagina });
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            TempData["MensagemErro"] = ex.Message;
            return RedirectToAction(nameof(Index), new { pesquisa, ativo, pagina });
        }

    }
}
