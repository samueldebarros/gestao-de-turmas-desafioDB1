using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository;

public class DisciplinaRepository : IDisciplinaRepository
{
    public readonly GestaoEscolarContext _context;

    public DisciplinaRepository(GestaoEscolarContext context)
    {
        _context = context;
    }

    public async Task AdicionarDisciplinaAsync(Disciplina disciplina)
    {
        _context.Add(disciplina);
        await _context.SaveChangesAsync();

    }

    public Task EditarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }

    public Task InativarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Disciplina> ObterDisciplinaPorIdAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Disciplina> ObterInativoPorIdAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<Disciplina>> ObterTodasAsDisciplinasAsync()
    {
        return await _context.Disciplinas.AsNoTracking().ToListAsync();
    }

    public Task ReativarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }
}
