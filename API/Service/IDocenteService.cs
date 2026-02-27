using API.DTOs.DocenteDTOs;
using Common.Domains;

namespace API.Service;

public interface IDocenteService
{
    Task AdicionarDocenteAsync(DocenteInputDTO docente);
    Task<List<Docente>> ObterTodosOsDocentesAsync();
    Task InativarDocenteAsync(int id);
    Task ReativarDocenteAsync(int id);
    Task<Docente> ObterPeloIdAsync(int id);
    Task EditarDocenteAsync(EditarDocenteDTO docente);
}
