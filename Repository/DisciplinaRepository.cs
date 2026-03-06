using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository;

public class DisciplinaRepository : IDisciplinaRepository
{
    private readonly GestaoEscolarContext _context;

    public DisciplinaRepository(GestaoEscolarContext context)
    {
        _context = context;
    }

    public async Task AdicionarDisciplinaAsync(Disciplina disciplina)
    {
        _context.Add(disciplina);
        await _context.SaveChangesAsync();

    }

    public async Task EditarDisciplinaAsync(Disciplina disciplina)
    {
        _context.Disciplinas.Update(disciplina);
        await _context.SaveChangesAsync();
    }

    public Task InativarDisciplinaAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Disciplina> ObterDisciplinaPorIdAsync(int id)
    {
        return await _context.Disciplinas.FirstOrDefaultAsync(d => d.Id == id && d.Ativo);
    }

    public async Task<Disciplina> ObterInativoPorIdAsync(int id)
    {
        return await _context.Disciplinas.FirstOrDefaultAsync(d => d.Id == id && !d.Ativo);
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
