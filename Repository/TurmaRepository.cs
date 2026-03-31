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

    private IQueryable<Turma> AplicarFiltros(IQueryable<Turma> query, string? pesquisa)
    {
        if (!string.IsNullOrEmpty(pesquisa))
        {
            var pesquisaTratada = pesquisa.ToLower().Trim().Replace("º", "");

            var series = Enum.GetValues<SerieEnum>()
            .Where(s => s.ObterSerieFormatada().ToLower().Trim().Replace("º", "").Contains(pesquisaTratada))
            .ToList();

            query = query.Where(t => t.Identificador.Contains(pesquisa) ||
                t.AnoLetivo.ToString().Contains(pesquisa) ||
                series.Contains(t.Serie));
        }

        return query;
    }

    private IQueryable<Turma> AplicarOrdenacao(IQueryable<Turma> query, OrdenacaoTurmaEnum? ordenacao)
    {
        return ordenacao switch
        {
            OrdenacaoTurmaEnum.Serie => query.OrderBy(t => t.Serie).ThenBy(t => t.Identificador),
            OrdenacaoTurmaEnum.Turno => query.OrderBy(t => t.Turno).ThenBy(t => t.Serie),
            _ => query.OrderByDescending(t => t.AnoLetivo).ThenBy(t => t.Serie).ThenBy(t => t.Identificador)
        };
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

    public async Task<List<TurmaResumo>> ObterTurmasSimplificadasAsync(string? pesquisa = null, OrdenacaoTurmaEnum? ordenacao = null)
    {
        var query = _context.Turmas.AsNoTracking().AsQueryable();

        if(pesquisa != null)
            query = AplicarFiltros(query, pesquisa);

        if(ordenacao != null)
            query = AplicarOrdenacao(query, ordenacao);

        return await query
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
            .ToListAsync();
    }

    public async Task<Turma> ObterTurmaPeloIdAsync(int id)
    {
        return await _context.Turmas.FirstOrDefaultAsync(t => t.Id == id);
    }
}