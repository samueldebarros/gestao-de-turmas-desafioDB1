using API.Service;
using Common.Enums;
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
    public async Task<IActionResult> Adicionar(TurmaInputViewModel model)
    {
        if (!ModelState.IsValid) return PartialView("_Adicionar", model);

        var turmaDto = model.ToInputDTO();

        await _turmaService.AdicionarTurmaAsync(turmaDto);
        TempData["MensagemSucesso"] = "Turma adicionada com sucesso!";
        return Json(new { sucesso = true });

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
    public async Task<IActionResult> Editar(TurmaEditarViewModel turma)
    {
        if (!ModelState.IsValid) return PartialView("_Editar", turma);

        var turmaDto = turma.ToEditarDTO();

        await _turmaService.EditarTurmaAsync(turmaDto);
        TempData["MensagemSucesso"] = "Turma editada com sucesso!";
        return Json(new { sucesso = true });
    }
}
