using Common.Domains;

namespace Repository;

public interface IBaseRepository<T> where T : class, IEntidade
{
    Task<T?> ObterPorIdAsync(int id);
    Task AdicionarAsync(T entidade);
    Task EditarAsync(T entidade);
}
