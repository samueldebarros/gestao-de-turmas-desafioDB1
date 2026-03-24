using Common.Domains;
using Common.Utils;
using Common.Enums;
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

    public async Task<bool> ExisteTurmaAsync(string identificador, int anoLetivo, SerieEnum serie, int? ignorarId = null)
    {
        var query = _context.Turmas
            .Where(t => t.Identificador == identificador &&
            t.AnoLetivo == anoLetivo &&
            t.Serie == serie);

        if (ignorarId.HasValue)
            query = query.Where(t => t.Id != ignorarId.Value);

        return await query.AnyAsync();
    }

    private IQueryable<Turma> AplicarFiltros(IQueryable<Turma> query, string? pesquisa, bool? ativo)
    {
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var pesquisaLower = pesquisa.ToLower().Trim().Replace("º", "");

            var seriesCompativeis = Enum.GetValues<SerieEnum>()
                .Where(s => s.ObterDescricao().ToLower().Replace("º", "").Contains(pesquisaLower))
                .ToList();

            query = query.Where(t =>
                t.Identificador.Contains(pesquisa) ||
                t.AnoLetivo.ToString().Contains(pesquisa) ||
                seriesCompativeis.Contains(t.Serie));
        }

        if (ativo.HasValue)
            query = query.Where(t => t.Ativo == ativo.Value);

        return query;
    }

    private IQueryable<Turma> AplicarOrdenacao(IQueryable<Turma> query, OrdenacaoTurmaEnum? ordenacao)
    {
        return ordenacao switch
        {
            OrdenacaoTurmaEnum.Serie => query.OrderBy(t => t.Serie)
                                             .ThenBy(t => t.Identificador),

            OrdenacaoTurmaEnum.Turno => query.OrderBy(t => t.Turno)
                                             .ThenBy(t => t.Serie),

            null => query.OrderByDescending(t => t.AnoLetivo)
                         .ThenBy(t => t.Serie)
                         .ThenBy(t => t.Identificador),

            _ => query.OrderByDescending(t => t.Id)
        };
    }

    public async Task<Turma?> ObterTurmaComDetalhesAsync(int id)
    {
        return await _context.Turmas
            .Include(t => t.Enturmamentos)
            .ThenInclude(t => t.Aluno)
            .Include(t => t.GradeCurricular)
            .ThenInclude(t => t.Disciplina)
            .Include(t => t.GradeCurricular)
            .ThenInclude(t => t.Docente)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Turma>> ObterTodasAsTurmasAsync(string? pesquisa = null, bool? ativo = null, OrdenacaoTurmaEnum? ordenacao = null)
    {
        var query = _context.Turmas
            .AsNoTracking()
            .Include(t => t.Enturmamentos)
            .Include(t => t.GradeCurricular)
            .AsQueryable();

        query = AplicarFiltros(query, pesquisa, ativo);
        query = AplicarOrdenacao(query, ordenacao);

        return await query.ToListAsync();

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
    public async Task AdicionarEnturmamentoAsync(Enturmamento enturmamento)
    {
        await _context.Enturmamentos.AddAsync(enturmamento);
        // Nota: Se você usa UnitOfWork, não chame o SaveChanges aqui. 
        // Se não usa, chame o SaveChanges para efetivar no banco.
        await _context.SaveChangesAsync();
    }

    public async Task AdicionarGradeCurricularAsync(GradeCurricular gradeCurricular)
    {
        await _context.GradeCurricular.AddAsync(gradeCurricular);
        await _context.SaveChangesAsync();
    }
}