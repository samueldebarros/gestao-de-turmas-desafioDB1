using API.Service;
using Common.Exceptions;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Disciplina;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestãoDeTurmas.Controllers;

public class GerenciarDisciplinaController : Controller
{
    private readonly IDisciplinaService _disciplinaService;
    private const int TAMANHO_PAGINA = 5;

    public GerenciarDisciplinaController(IDisciplinaService disciplinaService)
    {
        _disciplinaService = disciplinaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1, string? pesquisa = null, bool? ativo = null)
    {
        ViewBag.PesquisaAtual = pesquisa;
        ViewBag.AtivoAtual = ativo;

        var listaDisciplinas = await _disciplinaService.ObterTodasAsDisciplinasAsync(pagina, TAMANHO_PAGINA, pesquisa, ativo);

        if (pagina > listaDisciplinas.TotalPaginas && listaDisciplinas.TotalPaginas > 0)
            return RedirectToAction(nameof(Index), new { pagina = listaDisciplinas.TotalPaginas, pesquisa, ativo });

        var disciplinasDomain = listaDisciplinas.Select(d => d.ToListaViewModel()).ToList();

        var viewModel = new GerenciarDisciplinaViewModel()
        {
            DisciplinasCadastradas = disciplinasDomain,
            NovaDisciplina = new DisciplinaInputViewModel(),

            TemProximaPagina = listaDisciplinas.TemProximaPagina,
            TemPaginaAnterior = listaDisciplinas.TemPaginaAnterior,
            TotalResultados = listaDisciplinas.TotalResultados,
            PaginaAtual = pagina,
            TotalPaginas = listaDisciplinas.TotalPaginas,
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Adicionar()
    {
        return View(new DisciplinaInputViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adicionar(DisciplinaInputViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var disciplina = model.ToInputDTO();
        try
        {
            await _disciplinaService.AdicionarDisciplinaAsync(disciplina);
            return RedirectToAction(nameof(Index));
        }
        catch(RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);

        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Já existe uma disciplina com este nome.");
            return View(model);
        }

    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var disciplina = await _disciplinaService.ObterDisciplinaPorIdAsync(id);
        if (disciplina == null) return NotFound();

        var viewModel = disciplina.ToEditarViewModel();

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(DisciplinaEditarViewModel viewModel)
    {
        if (!ModelState.IsValid) return View(nameof(Editar), viewModel);

        var disciplina = viewModel.ToEditarDTO();
        try
        {
            await _disciplinaService.EditarDisciplinaAsync(disciplina);
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
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
    public async Task<IActionResult> Inativar(int id, string? pesquisa = null, bool? ativo = null, int pagina = 1)
    {
        try
        {
            await _disciplinaService.InativarDisciplinaAsync(id);
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
            return RedirectToAction(nameof(Index), new { pesquisa, ativo, pagina });
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            TempData["MensagemErro"] = ex.Message;
            return RedirectToAction(nameof(Index), new { pesquisa, ativo, pagina });
        }

    }
}
