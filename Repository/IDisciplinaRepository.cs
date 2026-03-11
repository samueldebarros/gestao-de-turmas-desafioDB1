using Common.Domains;

namespace Repository;

public interface IDisciplinaRepository
{
    Task<Disciplina> ObterDisciplinaPorIdAsync(int id);
    Task<Disciplina> ObterInativoPorIdAsync(int id);
    Task<(List<Disciplina>, int total)> ObterTodasAsDisciplinasAsync(int pagina = 1 , int tamanho = 5, string? pesquisa = null, bool? ativo = null);
    Task AdicionarDisciplinaAsync(Disciplina disciplina);
    Task EditarDisciplinaAsync(Disciplina disciplina);
    Task InativarDisciplinaAsync(int id);
    Task ReativarDisciplinaAsync(int id);
    Task<bool> ExistePeloNomeAsync(string nome, int? ignorarId = null);
    Task<List<Disciplina>> ObterDisciplinasAtivasAsync();
}
