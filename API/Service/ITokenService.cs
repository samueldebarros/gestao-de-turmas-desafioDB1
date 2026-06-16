using Common.Domains;

namespace API.Service;

public interface ITokenService
{
    public string GerarToken(Usuario usuario);
}
