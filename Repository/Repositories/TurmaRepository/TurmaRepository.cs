using Common.Domains;
using Common.Utils;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Relatorios;
using Repository.Repositories.Base;

namespace Repository.Repositories.TurmaRepository;

public class TurmaRepository : BaseRepository<Turma> , ITurmaRepository
{
    public TurmaRepository (GestaoEscolarContext context) : base(context)
    {
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
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (ignorarId != null)
        {
            query = query.Where(t => t.Id != ignorarId);
        }

        return query.Any(t => t.Identificador == identificador && t.Serie == serie && t.AnoLetivo == anoLetivo);
    }

    public async Task<List<Turma>> ObterTodasAsTurmasAsync()
    {
        return await _dbSet
            .Include(t => t.Enturmamentos).ThenInclude(e => e.Aluno)
            .Include(t => t.GradeCurricular).ThenInclude(g => g.Disciplina)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<List<TurmaResumo>> ObterTurmasSimplificadasAsync(string? pesquisa = null, OrdenacaoTurmaEnum? ordenacao = null)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

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

    public async Task<List<PainelDemograficoTurmaResultado>> ObterPainelDemograficoPorTurmaAsync()
    {
        return await _context.Database.SqlQuery<PainelDemograficoTurmaResultado>($@"
            SELECT
                t.Id AS TurmaId,
                t.Identificador AS Identificador,
                CAST(t.Serie AS INT) AS Serie,
                SUM(CASE WHEN ai.Idade < 15              THEN 1 ELSE 0 END) AS Menor15,
                SUM(CASE WHEN ai.Idade BETWEEN 15 AND 17 THEN 1 ELSE 0 END) AS De15a17,
                SUM(CASE WHEN ai.Idade >= 18             THEN 1 ELSE 0 END) AS Maior18,
                CAST(AVG(ai.Idade * 1.0) AS DECIMAL(5,2))                  AS IdadeMedia
            FROM Turmas t
            JOIN Enturmamentos e ON e.TurmaId = t.Id AND e.Situacao = {(int)SituacaoEnturmamentoEnum.Ativo}
            JOIN (
                SELECT
                    a.Id,
                    DATEDIFF(YEAR, a.DataNascimento, GETDATE())
                      - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, a.DataNascimento, GETDATE()), a.DataNascimento) > GETDATE()
                             THEN 1 ELSE 0 END AS Idade
                FROM Alunos a
            ) ai ON ai.Id = e.AlunoId
            GROUP BY t.Id, t.Identificador, t.Serie
            ORDER BY t.Serie, t.Identificador;").ToListAsync();
    }

    public async Task<List<BalancoEvasaoSerieResultado>> ObterBalancoEvasaoPorSerieAsync()
    {
        return await _context.Database.SqlQuery<BalancoEvasaoSerieResultado>($@"
            SELECT
                CAST(t.AnoLetivo AS INT) AS AnoLetivo,
                CAST(t.Serie AS INT) AS Serie,
                COUNT(e.AlunoId) AS TotalMatriculas,
                SUM(CASE WHEN e.Situacao = {(int)SituacaoEnturmamentoEnum.Ativo} THEN 1 ELSE 0 END) AS AlunosAtivos,
                SUM(CASE WHEN e.Situacao IN ({(int)SituacaoEnturmamentoEnum.Trancado}, {(int)SituacaoEnturmamentoEnum.Cancelado}) THEN 1 ELSE 0 END) AS AlunosTrancadosOuCancelados,
                CAST(
                    100.0 * SUM(CASE WHEN e.Situacao IN ({(int)SituacaoEnturmamentoEnum.Trancado}, {(int)SituacaoEnturmamentoEnum.Cancelado}) THEN 1 ELSE 0 END)
                    / NULLIF(COUNT(e.AlunoId), 0)
                    AS DECIMAL(5,2)
                ) AS PercentualEvasao
            FROM Turmas t
            JOIN Enturmamentos AS e ON t.Id = e.TurmaId
            JOIN Alunos AS a ON a.Id = e.AlunoId
            GROUP BY t.AnoLetivo, t.Serie
            ORDER BY t.AnoLetivo, t.Serie ASC;").ToListAsync();
    }

    public async Task<(List<TurmaResumo> lista, int total)> ObterTurmasPaginadasAsync(
                       int pagina, int tamanho, string? pesquisa, int? anoLetivo,
                       TurnoEnum? turno, bool? ativo, OrdenacaoTurmaEnum? ordenacao = null)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(pesquisa)) query = AplicarFiltros(query, pesquisa);
        if (anoLetivo.HasValue) query = query.Where(t => t.AnoLetivo == anoLetivo.Value);
        if (turno.HasValue) query = query.Where(t => t.Turno == turno.Value);
        if (ativo.HasValue) query = query.Where(t => t.Ativo == ativo.Value);

        int total = await query.CountAsync();

        query = AplicarOrdenacao(query, ordenacao);

        var lista = await query
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(t => new TurmaResumo
            {
                Id = t.Id,
                Identificador = t.Identificador,
                Turno = t.Turno,
                Serie = t.Serie,
                Capacidade = t.Capacidade,
                AnoLetivo = t.AnoLetivo,
                Ativo = t.Ativo,
                QuantidadeAlunos = t.Enturmamentos.Count,
                QuantidadeDisciplinas = t.GradeCurricular.Count
            })
            .ToListAsync();

        return (lista, total);
    }
}