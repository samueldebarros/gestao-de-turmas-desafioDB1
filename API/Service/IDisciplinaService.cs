using API.DTOs.DisciplinaDTOs;
using Common.Domains;

namespace API.Service;

public interface IDisciplinaService
{
    Task<Disciplina> ObterDisciplinaPorIdAsync();
    Task<Disciplina> ObterInativoPorIdAsync();
    Task<List<Disciplina>> ObterTodasAsDisciplinasAsync();
    Task AdicionarDisciplinaAsync(DisciplinaInputDTO disciplina);
    Task EditarDisciplinaAsync();
    Task InativarDisciplinaAsync();
    Task ReativarDisciplinaAsync();
}
