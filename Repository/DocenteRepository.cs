using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository;

public class DocenteRepository : IDocenteRepository
{
    private readonly GestaoEscolarContext _context;

    public DocenteRepository(GestaoEscolarContext context)
    {
        _context = context;
    }
    public async Task<Docente> ObterPeloIdAsync(int id)
    {
        return await _context.Docentes.FirstOrDefaultAsync(d => d.Id == id && d.Ativo);
    }
    public async Task<Docente> ObterInativoPeloIdAsync(int id)
    {
        return await _context.Docentes.FirstOrDefaultAsync(d => d.Id == id && !d.Ativo);
    }

    public async Task AdicionarDocenteAsync(Docente docente)
    {
        _context.Add(docente);

        await _context.SaveChangesAsync();
    }

    public async Task InativarDocenteAsync(int id)
    {
        await _context.Docentes
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(d => d.SetProperty(d => d.Ativo, false));
    }

    public async Task<(List<Docente>, int total)> ObterTodosOsDocentesAsync(int pagina = 1, int tamanho = 5, string? pesquisa = null, bool? ativo = null)
    {
        var query = _context.Docentes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(pesquisa))
        {
            var pesquisaLimpaCpf = pesquisa.Replace(".", "").Replace("-", "");
            query = query.Where(d => d.Nome.Contains(pesquisa)
                || d.Cpf.Contains(pesquisaLimpaCpf)
                || d.Especialidade.Contains(pesquisa));
        }

        if (ativo.HasValue) query = query.Where(d => d.Ativo == ativo.Value);

        int total = await query.CountAsync();

        var docentesPaginados = await query.OrderBy(d => d.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return (docentesPaginados, total);
    }

    public async Task ReativarDocenteAsync(int id)
    {
        await _context.Docentes
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(d => d.SetProperty(d => d.Ativo, true));
    }

    public async Task EditarDocenteAsync(Docente docente)
    {
        _context.Docentes.Update(docente);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistePeloCpfAsync(string cpf)
    {
        return await _context.Docentes.AnyAsync(d => d.Cpf == cpf);
    }
    public async Task<bool> ExistePeloEmailAsync(string email, int? ignorarId = null)
    {
        var query = _context.Docentes.Where(d => d.Email == email);

        if (ignorarId.HasValue)
            query = query.Where(d => d.Id != ignorarId.Value);

        return await query.AnyAsync();
    }
}
