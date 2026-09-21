using Common.Domains;

namespace Repository.Repositories.GradeCurricularRepository;

public interface IGradeCurricularRepository
{
    Task<GradeCurricular?> ObterAsync(int turmaId, int disciplinaId);
    Task AdicionarAsync(GradeCurricular grade);
    Task AtualizarAsync(GradeCurricular grade);
    Task RemoverAsync(GradeCurricular grade);
}
