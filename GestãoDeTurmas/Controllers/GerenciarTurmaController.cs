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
    public async Task<IActionResult> Index()
    {
        var turmas = await _turmaService.ObterTodasAsTurmasAsync();

        var listaViewModel = turmas.Select(t => t.ToListaViewModel()).ToList();

        return View(listaViewModel);
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
}
