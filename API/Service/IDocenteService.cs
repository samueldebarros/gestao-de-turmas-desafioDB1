using API.DTOs.DocenteDTOs;

namespace API.Service;

public interface IDocenteService
{
    Task AdicionarDocenteAsync(DocenteInputDTO docente);
}
