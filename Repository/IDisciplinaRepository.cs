using Common.Domains;

namespace Repository;

public interface IDisciplinaRepository
{
    Task<Disciplina> ObterDisciplinaPorIdAsync(int id);
    Task<Disciplina> ObterInativoPorIdAsync(int id);
    Task<List<Disciplina>> ObterTodasAsDisciplinasAsync();
    Task AdicionarDisciplinaAsync(Disciplina disciplina);
    Task EditarDisciplinaAsync(Disciplina disciplina);
    Task InativarDisciplinaAsync(int id);
    Task ReativarDisciplinaAsync(int id);
    
}
