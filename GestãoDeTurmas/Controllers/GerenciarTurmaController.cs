using API.Service;
using Common.Enums;
using Common.Exceptions;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Turma;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

public class GerenciarTurmaController : BaseController
{
    private readonly ITurmaService _turmaService;

    public GerenciarTurmaController(ITurmaService turmaService)
    {
        _turmaService = turmaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? pesquisa = null, OrdenacaoTurmaEnum? ordenacao = null)
    {
        var turmasExistentes = await _turmaService.ObterTurmasSimplificadasAsync(pesquisa, ordenacao);

        GerenciarTurmaViewModel turmaModel = new GerenciarTurmaViewModel
        {
            Turmas = turmasExistentes.Select(t => t.ToListaViewModel()).ToList(),
            Pesquisa = pesquisa,
            Ordenacao = ordenacao
        };

        return View(turmaModel);
    }

    [HttpGet]
    public IActionResult Adicionar()
    {
        return PartialView("_Adicionar", new TurmaInputViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adicionar(TurmaInputViewModel turmaModel)
    {
        if (!ModelState.IsValid) {
            Response.StatusCode = 400;
            return PartialView("_Adicionar", turmaModel);
        }

        try
        {
            await _turmaService.AdicionarTurmaAsync(turmaModel.ToInputDTO());
            return Json(new { sucesso = true });
        } catch(RegraDeNegocioException ex)
        {
            return TratarErroRegraDeNegocio(ex, "_Adicionar", turmaModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var turma = await _turmaService.ObterTurmaPeloIdAsync(id);
        if (turma == null) return NotFound();

        var turmaModel = turma.ToEditarViewModel();

        return PartialView("_Editar", turmaModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(TurmaEditarViewModel turmaModel)
    {
        if (!ModelState.IsValid) {
            Response.StatusCode = 400;
            return PartialView("_Editar", turmaModel); 
        }

        try
        {
            await _turmaService.EditarTurmaAsync(turmaModel.ToEditarDTO());
            TempData["MensagemSucesso"] = "Turma editada com sucesso!";
            return Json(new { sucesso = true });
        } catch(EntidadeNaoEncontradaException ex)
        {
            return TratarErroEntidadeNaoEncontrada(ex, "_Editar", turmaModel);
        }
        catch (RegraDeNegocioException ex)
        {
            return TratarErroRegraDeNegocio(ex, "_Editar", turmaModel);
        }
    }

}