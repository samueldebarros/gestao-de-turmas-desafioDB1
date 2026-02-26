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

    public async Task AdicionarDocenteAsync(Docente docente)
    {
        _context.Add(docente);

        await _context.SaveChangesAsync();
    }

    public async Task<List<Docente>> ObterTodosOsDocentesAsync()
    {
        return await _context.Docentes.AsNoTracking().ToListAsync();
    }
}
