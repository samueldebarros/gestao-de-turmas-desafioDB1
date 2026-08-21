using Common.Domains;
using Common.Enums;
using Repository.Relatorios;
using Repository.Repositories.Base;

namespace Repository.Repositories.TurmaRepository;

public interface ITurmaRepository : IBaseRepository<Turma>
{
    public Task<List<Turma>> ObterTodasAsTurmasAsync();
    Task<List<TurmaResumo>> ObterTurmasSimplificadasAsync(string? pesquisa = null, OrdenacaoTurmaEnum? ordenacao = null);
    Task<bool> ValidarPelosIdentificadores(string identificador, SerieEnum serie, int anoLetivo, int? ignorarId = null);
    Task<bool> ExisteAsync(int id);
    Task<List<DocenteSqlDto>> ObterDocentesDaTurmaAsync(int turmaId);
    Task<List<Aluno>> ObterAlunosDaTurmaAsync(int turmaId);
    Task<List<PainelDemograficoTurmaResultado>> ObterPainelDemograficoPorTurmaAsync();
    Task<List<BalancoEvasaoSerieResultado>> ObterBalancoEvasaoPorSerieAsync();
    Task<(List<TurmaResumo> lista, int total)> ObterTurmasPaginadasAsync(
    int pagina, int tamanho, string? pesquisa, int? anoLetivo,
    TurnoEnum? turno, bool? ativo, OrdenacaoTurmaEnum? ordenacao = null);
    Task<List<AlunoDeTurmaSqlDto>> ObterAlunosEmLoteAsync(IReadOnlyCollection<int> turmaIds);
    Task<List<DocenteDeTurmaSqlDto>> ObterDocentesEmLoteAsync(IReadOnlyCollection<int> turmaIds);

}