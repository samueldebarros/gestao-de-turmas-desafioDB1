using Common.Domains;
using System.Security.Claims;

namespace API.Service;

public interface ITokenService
{
    string GerarToken(Usuario usuario);
    string GerarRefreshToken(Usuario usuario);
    ClaimsPrincipal? ValidarRefreshToken(string token);
}
