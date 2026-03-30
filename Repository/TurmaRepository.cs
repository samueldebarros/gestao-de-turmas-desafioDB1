using Common.Domains;
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
    
    public async Task<bool> ValidarPelosIdentificadores(string identificador, SerieEnum serie, int anoLetivo, int? ignorarId = null)
    {
        var query = _context.Turmas.AsNoTracking().AsQueryable();

        if (ignorarId != null)
        {
            query = query.Where(t => t.Id != ignorarId);
        }

        return query.Any(t => t.Identificador == identificador && t.Serie == serie && t.AnoLetivo == anoLetivo);
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

    public async Task<List<Turma>> ObterTodasAsTurmasAsync()
    {
        return await _context.Turmas
            .Include(t => t.Enturmamentos).ThenInclude(e => e.Aluno)
            .Include(t => t.GradeCurricular).ThenInclude(g => g.Disciplina)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<List<TurmaResumo>> ObterTurmasSimplificadasAsync()
    {
        return await _context.Turmas
            .Select(t => new TurmaResumo()
            {
                Id = t.Id,
                Identificador = t.Identificador,
                Turno = t.Turno,
                Serie = t.Serie,
                Capacidade = t.Capacidade,
                AnoLetivo = t.AnoLetivo,
                QuantidadeAlunos = t.Enturmamentos.Count,
                QuantidadeDisciplinas = t.GradeCurricular.Count
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Turma> ObterTurmaPeloIdAsync(int id)
    {
        return await _context.Turmas.FirstOrDefaultAsync(t => t.Id == id);
    }
}