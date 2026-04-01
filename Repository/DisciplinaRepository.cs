using Common.Domains;
using Common.Enums;
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

    private IQueryable<Disciplina> OrdenarPor(IQueryable<Disciplina> query, string? ordenacao = null, DirecaoOrdenacaoEnum? direcao = null)
    {
        bool isDesc = direcao == DirecaoOrdenacaoEnum.Desc;

        return ordenacao switch
        {
            "Nome" => isDesc ? query.OrderByDescending(d => d.Nome) : query.OrderBy(d => d.Nome),
            "CargaHoraria" => isDesc ? query.OrderByDescending(d => d.CargaHoraria) : query.OrderBy(d => d.CargaHoraria),
            _ => query.OrderBy(d => d.Nome)
        };
    }

    public async Task AdicionarDisciplinaAsync(Disciplina disciplina)
    {
        _context.Add(disciplina);
        await _context.SaveChangesAsync();

    }

    public async Task<List<Disciplina>> ObterDisciplinasDisponiveisParaTurmaAsync(int turmaId)
    {
        var discplinasJaVinculadas = _context.GradeCurricular
            .Where(g => g.TurmaId == turmaId)
            .Select(g => g.DisciplinaId);

        return await _context.Disciplinas
            .AsNoTracking()
            .Where(d => d.Ativo && !discplinasJaVinculadas.Contains(d.Id))
            .OrderBy(d => d.Nome)
            .ToListAsync();
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

    public async Task<(List<Disciplina>, int total)> ObterTodasAsDisciplinasAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null, string? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null)
    {
        var query = _context.Disciplinas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(pesquisa))
            query = query.Where(d => d.Nome.Contains(pesquisa)
            || d.Ementa.Contains(pesquisa));

        if (ativo.HasValue) query = query.Where(d => d.Ativo == ativo.Value);

        int total = await query.CountAsync();

        query = OrdenarPor(query, ordenacao, direcao);

        var disciplinas = await query.Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return (disciplinas, total);
    }

    public async Task ReativarDisciplinaAsync(int id)
    {
        await _context.Disciplinas
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(d => d.SetProperty(d => d.Ativo, true));
    }

    public async Task<bool> ExistePeloNomeAsync(string nome, int? ignorarId = null)
    {
        var query = _context.Disciplinas
            .Where(d => d.Nome.ToLower() == nome.ToLower());

        if (ignorarId.HasValue)
            query = query.Where(d => d.Id != ignorarId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExisteAtivaAsync(int id)
    {
        return await _context.Disciplinas.AnyAsync(d => d.Id == id && d.Ativo);
    }

    public async Task<bool> PossuiDocentesAtivosAsync(int disciplinaId)
    {
        return await _context.Docentes.AnyAsync(d => d.DisciplinaId == disciplinaId && d.Ativo);
    }

    public async Task<List<Disciplina>> ObterDisciplinasAtivasAsync()
    {
        return await _context.Disciplinas
            .AsNoTracking()
            .Where(d => d.Ativo)
            .OrderBy(d => d.Nome)
            .ToListAsync();
    }

}
