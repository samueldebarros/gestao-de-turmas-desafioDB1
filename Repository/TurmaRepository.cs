using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository;

public class TurmaRepository : ITurmaRepository
{
    private readonly GestaoEscolarContext _context;

    public TurmaRepository(GestaoEscolarContext context)
    {
        _context = context;
    }

    public async Task<List<Turma>> ObterTodasAsTurmasAsync()
    {
        return await _context.Turmas
            .AsNoTracking()
            .Include(t => t.Enturmamentos)
            .Include(t => t.GradeCurricular)
            .ToListAsync();
    }

    public async Task<Turma?> ObterPorIdAsync(int id)
    {
        return await _context.Turmas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AdicionarTurmaAsync(Turma turma)
    {
        await _context.Turmas.AddAsync(turma);
        await _context.SaveChangesAsync();
    }

    public async Task EditarTurmaAsync(Turma turma)
    {
        _context.Turmas.Update(turma);
        await _context.SaveChangesAsync();
    }

    public async Task InativarTurmaAsync(int id)
    {
        await _context.Turmas
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(t => t.SetProperty(t => t.Ativo, false));
    }

    public async Task ReativarTurmaAsync(int id)
    {
        await _context.Turmas
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(t => t.SetProperty(t => t.Ativo, true));
    }
}