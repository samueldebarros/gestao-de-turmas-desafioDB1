using API.Service;
using Common.Exceptions;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Turma;
using Microsoft.AspNetCore.Authentication;
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
        var turmasExistentes = await _turmaService.ObterTurmasSimplificadasAsync();

        GerenciarTurmaViewModel turmaModel = new GerenciarTurmaViewModel
        {
            Turmas = turmasExistentes.Select(t => t.ToListaViewModel()).ToList()
        };

        return View(turmaModel);
    }

    [HttpGet]
    public IActionResult Adicionar()
    {
        return PartialView("_Adicionar", new TurmaInputViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(TurmaInputViewModel turmaModel)
    {
        if (!ModelState.IsValid) return PartialView("_Adicionar", turmaModel);

        try
        {
            await _turmaService.AdicionarTurmaAsync(turmaModel.ToInputDTO());
            return Json(new { sucesso = true });
        } catch(RegraDeNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return PartialView("_Adicionar", turmaModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var turma = await _turmaService.ObterTurmaPeloIdAsync(id);

        var turmaModel = turma.ToEditarViewModel();

        return PartialView("_Editar", turmaModel);
    }

    [HttpPost]
    public async Task<IActionResult> Editar(TurmaEditarViewModel turmaModel)
    {
        if (!ModelState.IsValid) return PartialView("_Editar", turmaModel);

        try
        {
            await _turmaService.EditarTurmaAsync(turmaModel.ToEditarDTO());
            TempData["MensagemSucesso"] = "Turma editada com sucesso!";
            return Json(new { sucesso = true });
        } catch(EntidadeNaoEncontradaException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return PartialView("_Editar", turmaModel);
        }
    }

}