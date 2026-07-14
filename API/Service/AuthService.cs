using API.DTOs.AuthDTOs;
using Common.Domains;
using Microsoft.AspNetCore.Identity;
using Repository.Repositories.UsuarioRepository;
using System.Security.Claims;

namespace API.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly ITokenService _tokenService;

        public AuthService(IUsuarioRepository usuarioRepository, IPasswordHasher<Usuario> hasher, ITokenService tokenService)
        {
            _usuarioRepository = usuarioRepository;
            _hasher = hasher;
            _tokenService = tokenService;
        }

        public async Task<Usuario?> ValidarCredenciaisAsync(string email, string senha)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario is null) return null;
            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
            return resultado == PasswordVerificationResult.Success ? usuario : null;
        }

        public async Task<RenovacaoSessaoResultado?> RenovarSessaoAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var principal = _tokenService.ValidarRefreshToken(refreshToken);
            if (principal is null)
                return null;

            if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
                return null;

            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
            if (usuario is null)
                return null;

            return new RenovacaoSessaoResultado(
                _tokenService.GerarToken(usuario),
                _tokenService.GerarRefreshToken(usuario),
                usuario.Role);
        }
    }
}
