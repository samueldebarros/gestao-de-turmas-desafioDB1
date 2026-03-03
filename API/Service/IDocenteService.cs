using API.DTOs;
using API.DTOs.DocenteDTOs;
using Common.Domains;

namespace API.Service;

public interface IDocenteService
{
    Task AdicionarDocenteAsync(DocenteInputDTO docente);
    Task<ListaPaginada<Docente>> ObterTodosOsDocentesAsync(int pagina = 1, int tamanho = 5,string? pesquisa = null, bool? ativo = null);
    Task InativarDocenteAsync(int id);
    Task ReativarDocenteAsync(int id);
    Task<Docente> ObterPeloIdAsync(int id);
    Task EditarDocenteAsync(EditarDocenteDTO docente);
}
