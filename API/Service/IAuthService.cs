using API.DTOs.AuthDTOs;
using Common.Domains;

namespace API.Service;

public interface IAuthService
{
    Task<Usuario?> ValidarCredenciaisAsync(string email, string senha);
    Task<RenovacaoSessaoResultado?> RenovarSessaoAsync(string? refreshToken);
}
