using API.Service;
using Common.Enums;
using Common.Exceptions;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Turma;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

public class GerenciarTurmaController : Controller
{
    private readonly ITurmaService _turmaService;

    public GerenciarTurmaController(ITurmaService turmaService)
    {
        _turmaService = turmaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string pesquisa = null, bool? ativo = null)
    {
        var turmas = await _turmaService.ObterTodasAsTurmasAsync();

        var turmasViewModel = new GerenciarTurmaViewModel {
            TurmasCadastradas = turmas.Select(t => t.ToListaViewModel()).ToList(),
            Pesquisa = pesquisa,
            Ativo = ativo
        };

        return View(turmasViewModel);
    }

    [HttpGet]
    public IActionResult Adicionar()
    {
        return PartialView("_Adicionar", new TurmaInputViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adicionar(TurmaInputViewModel model)
    {
        if (!ModelState.IsValid) return PartialView("_Adicionar", model);

        try
        {
            await _turmaService.AdicionarTurmaAsync(model.ToInputDTO());
            TempData["MensagemSucesso"] = "Turma cadastrada com sucesso!";
            return Json(new { sucesso = true });
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return PartialView("_Adicionar", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var turma = await _turmaService.ObterPorIdAsync(id);
        if (turma == null) return NotFound();

        TurmaEditarViewModel turmaExistente = turma.ToEditarViewModel();

        return PartialView("_Editar", turmaExistente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(TurmaEditarViewModel model)
    {
        if (!ModelState.IsValid) return PartialView("_Editar", model);

        try
        {
            await _turmaService.EditarTurmaAsync(model.ToEditarDTO());
            TempData["MensagemSucesso"] = "Turma editada com sucesso!";
            return Json(new { sucesso = true });
        }
        catch (RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return PartialView("_Editar", model);
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }
}
