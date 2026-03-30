using Common.Domains;

namespace Repository;

public interface ITurmaRepository
{
    public Task<List<Turma>> ObterTodasAsTurmasAsync();
    Task AdicionarTurmaAsync(Turma turma);
    Task<Turma> ObterTurmaPeloIdAsync(int id);
    Task<List<TurmaResumo>> ObterTurmasSimplificadasAsync();
    Task EditarTurmaAsync(Turma turma);
}