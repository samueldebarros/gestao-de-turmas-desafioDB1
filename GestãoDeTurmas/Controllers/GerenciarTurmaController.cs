using API.Service;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

public class GerenciarTurmaController : Controller
{
    private readonly ITurmaService _turmaService;

    public GerenciarTurmaController(ITurmaService turmaService)
    {
        _turmaService = turmaService;
    }

    public IActionResult Index()
    {
        return View();
    }
}