using API.DTOs.AuthDTOs;
using API.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestãoDeTurmas.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string CookieAccess = "access_token";
    private const string CookieRefresh = "refresh_token";
    private const string CaminhoRefresh = "/api/auth";

    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginInput input)
    {
        var usuario = await _authService.ValidarCredenciaisAsync(input.Email, input.Senha);
        if (usuario is null) return Unauthorized();

        DefinirCookieAcesso(_tokenService.GerarToken(usuario));
        DefinirCookieRefresh(_tokenService.GerarRefreshToken(usuario));
        return Ok(new UsuarioPublicoOutput(usuario.Role));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var resultado = await _authService.RenovarSessaoAsync(Request.Cookies[CookieRefresh]);
        if (resultado is null) return Unauthorized();

        DefinirCookieAcesso(resultado.AccessToken);
        DefinirCookieRefresh(resultado.RefreshToken);
        return Ok(new UsuarioPublicoOutput(resultado.Role));
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        RemoverCookiesSessao();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role is null) return Forbid();
        return Ok(new UsuarioPublicoOutput(role));
    }

    private void DefinirCookieAcesso(string token) =>
        Response.Cookies.Append(CookieAccess, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15),
        });

    private void DefinirCookieRefresh(string token) =>
        Response.Cookies.Append(CookieRefresh, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = CaminhoRefresh,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
        });

    private void RemoverCookiesSessao()
    {
        Response.Cookies.Delete(CookieAccess, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
        });
        Response.Cookies.Delete(CookieRefresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = CaminhoRefresh,
        });
    }
}
