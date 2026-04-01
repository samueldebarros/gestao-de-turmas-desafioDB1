using Common.Domains;
using Common.Enums;

namespace Repository;

public interface IDisciplinaRepository
{
    Task<List<Disciplina>> ObterDisciplinasDisponiveisParaTurmaAsync(int turmaId);
    Task<Disciplina> ObterDisciplinaPorIdAsync(int id);
    Task<Disciplina> ObterInativoPorIdAsync(int id);
    Task<(List<Disciplina>, int total)> ObterTodasAsDisciplinasAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null, string? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null);
    Task AdicionarDisciplinaAsync(Disciplina disciplina);
    Task EditarDisciplinaAsync(Disciplina disciplina);
    Task InativarDisciplinaAsync(int id);
    Task ReativarDisciplinaAsync(int id);
    Task<bool> ExistePeloNomeAsync(string nome, int? ignorarId = null);
    Task<List<Disciplina>> ObterDisciplinasAtivasAsync();
    Task<bool> PossuiDocentesAtivosAsync(int disciplinaId);
    Task<bool> ExisteAtivaAsync(int id);
}
