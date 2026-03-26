using Common.Domains;
using Common.Utils;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository;

public class TurmaRepository : ITurmaRepository
{
    private readonly GestaoEscolarContext _context;

    public TurmaRepository (GestaoEscolarContext context)
    {
        _context = context;
    }

    public async Task<List<Turma>> ObterTodasAsTurmasAsync()
    {
        return await _context.Turmas
            .Include(t => t.Enturmamentos).ThenInclude(e => e.Aluno)
            .Include(t => t.GradeCurricular).ThenInclude(g => g.Disciplina)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }
}