using Common.Domains;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Repositories.Base;

namespace Repository.Repositories.DocenteRepository;

public class DocenteRepository : BaseInativavelRepository<Docente>, IDocenteRepository
{
    public DocenteRepository(GestaoEscolarContext context) : base(context)
    {
    }

    private IQueryable<Docente> OrdenarPor(IQueryable<Docente> query, string? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null)
    {
        bool IsDesc = direcao == DirecaoOrdenacaoEnum.Desc;

        return ordenacao switch
        {
            "Nome" => IsDesc ? query.OrderByDescending(d => d.Nome) : query.OrderBy(d => d.Nome),
            "Disciplina" => IsDesc ? query.OrderByDescending(d => d.Disciplina.Nome) : query.OrderBy(d => d.Disciplina.Nome),
            "DataNascimento" => IsDesc ? query.OrderByDescending(d => d.DataNascimento) : query.OrderBy(d => d.DataNascimento),
            _ => query.OrderBy(d => d.Nome)
        };
    }

    public override async Task<Docente> ObterPorIdAsync(int id)
    {
        return await _dbSet
            .Include(d => d.Disciplina)
            .FirstOrDefaultAsync(d => d.Id == id && d.Ativo);
    }

    public override async Task InativarAsync(int id)
    {
        await _dbSet
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(d => d
            .SetProperty(d => d.Ativo, false)
            .SetProperty(d => d.DisciplinaId, (int?)null));
    }

    public async Task<List<Docente>> ObterDocentesPorDisciplinaAsync(int disciplinaId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.DisciplinaId == disciplinaId)
            .OrderBy(d => d.Nome)
            .ToListAsync();
    }

    public async Task<(List<Docente>, int total)> ObterTodosOsDocentesAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null, string? ordenacao = null,
            DirecaoOrdenacaoEnum? direcao = null)
    {
        var query = _dbSet.Include(d => d.Disciplina).AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(pesquisa))
        {
            var pesquisaLimpaCpf = pesquisa.Replace(".", "").Replace("-", "");
            query = query.Where(d => d.Nome.Contains(pesquisa)
                || d.Cpf.Contains(pesquisaLimpaCpf)
                || d.Disciplina.Nome.Contains(pesquisa));
        }

        if (ativo.HasValue) query = query.Where(d => d.Ativo == ativo.Value);

        int total = await query.CountAsync();

        query = OrdenarPor(query, ordenacao, direcao);

        var docentesPaginados = await query.Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return (docentesPaginados, total);
    }

    public async Task<bool> ExistePeloCpfAsync(string cpf)
    {
        return await _dbSet.AnyAsync(d => d.Cpf == cpf);
    }
    public async Task<bool> ExistePeloEmailAsync(string email, int? ignorarId = null)
    {
        var query = _dbSet.Where(d => d.Email == email);

        if (ignorarId.HasValue)
            query = query.Where(d => d.Id != ignorarId.Value);

        return await query.AnyAsync();
    }
}
