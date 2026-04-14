using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository.Repositories.Base;

public abstract class BaseInativavelRepository<T> : BaseRepository<T>, IBaseInativavelRepository<T> where T : class, IEntidadeInativavel
{
    protected BaseInativavelRepository(GestaoEscolarContext context) : base(context) { }

    public virtual async Task InativarAsync(int id)
    {
        await _dbSet.Where(t => t.Id == id)
           .ExecuteUpdateAsync(t => t.SetProperty(t => t.Ativo, false));
    }

    public virtual async Task<T?> ObterInativoPorIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Id == id && t.Ativo == false);
    }

    public virtual async Task ReativarAsync(int id)
    {
        await _dbSet.Where(t => t.Id == id)
            .ExecuteUpdateAsync(t => t.SetProperty(t => t.Ativo, true));
    }
}
