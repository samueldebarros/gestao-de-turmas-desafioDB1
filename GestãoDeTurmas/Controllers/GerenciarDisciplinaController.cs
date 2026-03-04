using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

public class GerenciarDisciplinaController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

}
