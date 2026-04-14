using Common.Domains;

namespace Repository.Repositories.Base;

public interface IBaseInativavelRepository<T> : IBaseRepository<T> where T : class , IEntidadeInativavel
{

    Task<T?> ObterInativoPorIdAsync(int id);
    Task InativarAsync(int id);
    Task ReativarAsync(int id);
    Task<int> ContarAtivosAsync();
    Task<int> ContarInativosAsync();
}
