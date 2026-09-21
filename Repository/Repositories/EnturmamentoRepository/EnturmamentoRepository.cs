using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Repository.Context;

namespace Repository.Repositories.EnturmamentoRepository;

public class EnturmamentoRepository : IEnturmamentoRepository
{
    private readonly GestaoEscolarContext _context;

    public EnturmamentoRepository(GestaoEscolarContext context)
    {
        _context = context;
    }

    public async Task<Enturmamento?> ObterAsync(int turmaId, int alunoId)
    {
        return await _context.Enturmamentos
            .FirstOrDefaultAsync(e => e.TurmaId == turmaId && e.AlunoId == alunoId);
    }

    public async Task AdicionarAsync(Enturmamento enturmamento)
    {
        await _context.Enturmamentos.AddAsync(enturmamento);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Enturmamento enturmamento)
    {
        _context.Enturmamentos.Update(enturmamento);
        await _context.SaveChangesAsync();
    }
}
