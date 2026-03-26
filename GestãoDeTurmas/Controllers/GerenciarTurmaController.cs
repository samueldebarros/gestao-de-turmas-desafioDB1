using API.Service;
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
        var turmasExistentes = await _turmaService.ObterTodasAsTurmasAsync();

        GerenciarTurmaViewModel turmaModel = new GerenciarTurmaViewModel
        {
            Turmas = turmasExistentes.Select(t => t.ToListaViewModel()).ToList()
        };

        return View(turmaModel);
    }
}