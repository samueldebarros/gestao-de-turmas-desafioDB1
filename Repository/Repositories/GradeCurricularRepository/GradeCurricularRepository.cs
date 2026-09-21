using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository.Repositories.GradeCurricularRepository;

public class GradeCurricularRepository : IGradeCurricularRepository
{
    private readonly GestaoEscolarContext _context;

    public GradeCurricularRepository(GestaoEscolarContext context)
    {
        _context = context;
    }

    public async Task<GradeCurricular?> ObterAsync(int turmaId, int disciplinaId)
    {
        return await _context.GradeCurricular
            .FirstOrDefaultAsync(g => g.TurmaId == turmaId && g.DisciplinaId == disciplinaId);
    }

    public async Task AdicionarAsync(GradeCurricular grade)
    {
        await _context.GradeCurricular.AddAsync(grade);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(GradeCurricular grade)
    {
        _context.GradeCurricular.Update(grade);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(GradeCurricular grade)
    {
        _context.GradeCurricular.Remove(grade);
        await _context.SaveChangesAsync();
    }
}
