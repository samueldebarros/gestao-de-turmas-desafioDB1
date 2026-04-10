using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers;

public abstract class BaseController : Controller
{
    protected IActionResult TratarErroRegraDeNegocio(RegraDeNegocioException ex, string partialView, object model)
    {
        Response.StatusCode = 400;
        ModelState.AddModelError(string.Empty, ex.Message);
        return PartialView(partialView, model);
    }

    protected IActionResult TratarErroEntidadeNaoEncontrado(EntidadeNaoEncontradaException ex, string? partialView = null, object? model = null)
    {
        Response.StatusCode = 404;

        if (partialView != null && model != null)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return PartialView(partialView, model);
        }

        return Json(new { sucesso = false, mensagem = ex.Message });
    }
}
