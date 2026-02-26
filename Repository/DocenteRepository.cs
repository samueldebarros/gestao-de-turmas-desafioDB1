using Common.Domains;
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
}
