using Common.Domains;

namespace API.Service;

public interface IAuthService
{
    Task<Usuario?> ValidarCredenciaisAsync(string email, string senha);
}
