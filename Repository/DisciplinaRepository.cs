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

    public async Task InativarDisciplinaAsync(int id)
    {
        await _context.Disciplinas
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(d => d.SetProperty(d => d.Ativo, false));
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

    public Task ReativarDisciplinaAsync(int id)
    {
        throw new NotImplementedException();
    }
}
