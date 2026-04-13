using Common.Domains;
using Common.Enums;

namespace Repository;

public interface ITurmaRepository : IBaseRepository<Turma>
{
    public Task<List<Turma>> ObterTodasAsTurmasAsync();
    Task<List<TurmaResumo>> ObterTurmasSimplificadasAsync(string? pesquisa = null, OrdenacaoTurmaEnum? ordenacao = null);
    Task<bool> ValidarPelosIdentificadores(string identificador, SerieEnum serie, int anoLetivo, int? ignorarId = null);
}