using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers
{
    public class PaginaInicialController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
