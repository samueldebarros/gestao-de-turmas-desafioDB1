using API.DTOs.DisciplinaDTOs;
using Common.Domains;

namespace API.Service;

public interface IDisciplinaService
{
    Task<Disciplina> ObterDisciplinaPorIdAsync(int id);
    Task<Disciplina> ObterInativoPorIdAsync(int id);
    Task<List<Disciplina>> ObterTodasAsDisciplinasAsync();
    Task AdicionarDisciplinaAsync(DisciplinaInputDTO disciplina);
    Task EditarDisciplinaAsync(EditarDisciplinaDTO disciplinaDTO);
    Task InativarDisciplinaAsync(int id);
    Task ReativarDisciplinaAsync(int id);
}
