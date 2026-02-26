using API.DTOs.DocenteDTOs;
using Common.Domains;

namespace API.Service;

public interface IDocenteService
{
    Task AdicionarDocenteAsync(DocenteInputDTO docente);

    Task<List<Docente>> ObterTodosOsDocentesAsync();
}
