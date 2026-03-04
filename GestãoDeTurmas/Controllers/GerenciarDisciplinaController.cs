using API.Service;
using GestãoDeTurmas.Mappers;
using GestãoDeTurmas.Models.Disciplina;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

public class GerenciarDisciplinaController : Controller
{
    private readonly IDisciplinaService _disciplinaService;

    public GerenciarDisciplinaController(IDisciplinaService disciplinaService)
    {
        _disciplinaService = disciplinaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var listaDisciplinas = await _disciplinaService.ObterTodasAsDisciplinasAsync();

        var disciplinasDomain = listaDisciplinas.Select(d => d.ToListaViewModel()).ToList();

        var viewModel = new GerenciarDisciplinaViewModel()
        {
            DisciplinasCadastradas = disciplinasDomain,
            NovaDisciplina = new DisciplinaInputViewModel()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Adicionar()
    {
        return View(new DisciplinaInputViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(DisciplinaInputViewModel model)
    {
        var docente = model.ToInputDTO();

        await _disciplinaService.AdicionarDisciplinaAsync(docente);

        return RedirectToAction(nameof(Index));
    }

}
