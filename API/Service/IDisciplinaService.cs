using API.DTOs.DisciplinaDTOs;
using Common.Domains;

namespace API.Service;

public interface IDisciplinaService
{
    Task<Disciplina> ObterDisciplinaPorIdAsync();
    Task<Disciplina> ObterInativoPorIdAsync();
    Task<(List<Disciplina>, int total)> ObterTodasAsDisciplinasAsync();
    Task AdicionarDisciplinaAsync(DisciplinaInputDTO disciplina);
    Task EditarDisciplinaAsync();
    Task InativarDisciplinaAsync();
    Task ReativarDisciplinaAsync();
}
