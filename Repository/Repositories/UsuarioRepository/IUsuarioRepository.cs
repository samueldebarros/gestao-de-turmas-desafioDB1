
using Common.Domains;
using Repository.Repositories.Base;

namespace Repository.Repositories.UsuarioRepository;

public interface IUsuarioRepository : IBaseRepository<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email);
}
