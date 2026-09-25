using Common.Domains;

namespace Repository.Repositories.EnturmamentoRepository;

public interface IEnturmamentoRepository
{
    Task<Enturmamento?> ObterAsync(int turmaId, int alunoId);
    Task AdicionarAsync(Enturmamento enturmamento);
    Task AtualizarAsync(Enturmamento enturmamento);
}
