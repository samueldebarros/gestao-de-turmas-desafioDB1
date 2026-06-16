using Common.Domains;
using Microsoft.AspNetCore.Identity;
using Repository.Repositories.UsuarioRepository;

namespace API.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHasher<Usuario> _hasher;
        
        public AuthService(IUsuarioRepository usuarioRepository, IPasswordHasher<Usuario> hasher)
        {
            _usuarioRepository = usuarioRepository;
            _hasher = hasher;
        }

        public async Task<Usuario?> ValidarCredenciaisAsync(string email, string senha)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
            if (usuario is null) return null;
            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
            return resultado == PasswordVerificationResult.Success ? usuario : null;
        }
    }
}
