using Common.Domains;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using Repository.Repositories.Base;

namespace Repository.Repositories.UsuarioRepository;

public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(GestaoEscolarContext context) : base(context) { }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }
}
