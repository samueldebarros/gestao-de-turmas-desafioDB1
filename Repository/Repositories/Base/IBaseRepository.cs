using Common.Domains;

namespace Repository.Repositories.Base;

public interface IBaseRepository<T> where T : class, IEntidade
{
    Task<T?> ObterPorIdAsync(int id);
    Task AdicionarAsync(T entidade);
    Task EditarAsync(T entidade);
    Task<int> ContarTodosAsync();
}
