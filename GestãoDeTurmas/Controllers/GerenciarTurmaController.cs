using API.DTOs.TurmaDTOs;
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
    public async Task<IActionResult> Index(string? pesquisa = null, bool? ativo = null, OrdenacaoTurmaEnum? ordenacao = null)
    {
        var turmas = await _turmaService.ObterTodasAsTurmasAsync(pesquisa, ativo, ordenacao);

        var turmasViewModel = new GerenciarTurmaViewModel {
            TurmasCadastradas = turmas.Select(t => t.ToListaViewModel()).ToList(),
            Pesquisa = pesquisa,
            Ativo = ativo,
            Ordenacao = ordenacao
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

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        try
        {
            var turma = await _turmaService.ObterTurmaComDetalhesAsync(id);
            var alunosDisponiveis = await _turmaService.ObterAlunosDisponiveisParaTurmaAsync(id);
            var disciplinasDisponiveis = await _turmaService.ObterDisciplinasDisponiveisParaTurmaAsync(id);

            var viewModel = turma.ToDetalhesViewModel(alunosDisponiveis, disciplinasDisponiveis);

            return View(viewModel);
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            TempData["MensagemErro"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ObterDocentesPorDisciplina(int disciplinaId)
    {
        var docentes = await _turmaService.ObterDocentesPorDisciplinaAsync(disciplinaId);
        return Json(docentes.Select(d => new { id = d.Id, nome = d.Nome }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VincularDisciplina([FromForm] VincularDisciplinaDTO dto)
    {
        try
        {
            await _turmaService.VincularDisciplinaAsync(dto);
            return Json(new { sucesso = true });
        }
        catch (RegraDeNegocioException ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

}
