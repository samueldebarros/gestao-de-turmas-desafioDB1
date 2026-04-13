using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class, IEntidade
{
    protected readonly GestaoEscolarContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(GestaoEscolarContext context)
    {
        _context = context; 
        _dbSet = context.Set<T>();
    }

    public virtual async Task AdicionarAsync(T entidade)
    {
        _dbSet.Add(entidade);
        await _context.SaveChangesAsync();
    }

    public virtual async Task EditarAsync(T entidade)
    {
        _dbSet.Update(entidade);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<T?> ObterPorIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Id == id);
    }
}
