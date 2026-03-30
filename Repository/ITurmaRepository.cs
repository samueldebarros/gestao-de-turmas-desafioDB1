using Common.Domains;
using Common.Enums;

namespace Repository;

public interface ITurmaRepository
{
    public Task<List<Turma>> ObterTodasAsTurmasAsync();
    Task AdicionarTurmaAsync(Turma turma);
    Task<Turma> ObterTurmaPeloIdAsync(int id);
    Task<List<TurmaResumo>> ObterTurmasSimplificadasAsync(string? pesquisa = null, OrdenacaoTurmaEnum? ordenacao = null);
    Task EditarTurmaAsync(Turma turma);
    Task<bool> ValidarPelosIdentificadores(string identificador, SerieEnum serie, int anoLetivo, int? ignorarId = null);
}