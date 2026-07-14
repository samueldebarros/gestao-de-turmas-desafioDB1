using Common.Domains;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Service;

public class TokenService : ITokenService
{
    private const string ClaimTipoToken = "token_type";
    private const string TipoAccess = "access";
    private const string TipoRefresh = "refresh";

    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GerarToken(Usuario usuario)
        => GerarJwt(usuario, TipoAccess, TimeSpan.FromMinutes(MinutosAccess));

    public string GerarRefreshToken(Usuario usuario)
        => GerarJwt(usuario, TipoRefresh, TimeSpan.FromDays(DiasRefresh));

    public ClaimsPrincipal? ValidarRefreshToken(string token)
    {
        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, ParametrosValidacao(), out _);

            return principal.FindFirstValue(ClaimTipoToken) == TipoRefresh
                ? principal
                : null;
        }
        catch
        {
            return null;
        }
    }

    private string GerarJwt(Usuario usuario, string tipoToken, TimeSpan duracao)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Role),
            new Claim(ClaimTipoToken, tipoToken),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(duracao),
            signingCredentials: new SigningCredentials(Chave(), SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private TokenValidationParameters ParametrosValidacao() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _config["Jwt:Issuer"],
        ValidAudience = _config["Jwt:Audience"],
        IssuerSigningKey = Chave(),
        ClockSkew = TimeSpan.Zero,
    };

    private SymmetricSecurityKey Chave()
        => new(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

    private int MinutosAccess
        => int.TryParse(_config["Jwt:ExpireMinutes"], out var minutos) ? minutos : 15;

    private int DiasRefresh
        => int.TryParse(_config["Jwt:RefreshExpireDays"], out var dias) ? dias : 7;
}
