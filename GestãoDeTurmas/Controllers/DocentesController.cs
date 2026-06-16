using API.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestãoDeTurmas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Coordenador")]
    public class DocentesController : ControllerBase
    {
        private readonly IDocenteService _docenteService;
        private readonly string mensagemStatus500 = "Ocorreu um erro ao processar a requisição";

        public DocentesController(IDocenteService docenteService) {
            _docenteService = docenteService;
        }

        [HttpGet]
        public async Task<IActionResult> ObterDocentesDisciplinasAsync()
        {
            try
            {
                var listaDocentesComDiscplinas = await _docenteService.ObterDocentesDisciplinasSqlAsync();
                return Ok(listaDocentesComDiscplinas);
            }
            catch (Exception)
            {
                return StatusCode(500, mensagemStatus500);
            }

        }
    }
}
