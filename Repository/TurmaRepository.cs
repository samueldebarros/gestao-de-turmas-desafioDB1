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

    public async Task<List<Turma>> ListarAsync()
    {
        return await _context.Turmas
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Turma?> ObterPorIdAsync(int id)
    {
        return await _context.Turmas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AdicionarAsync(Turma turma)
    {
        await _context.Turmas.AddAsync(turma);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Turma turma)
    {
        _context.Turmas.Update(turma);
        await _context.SaveChangesAsync();
    }
}