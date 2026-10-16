using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Application.Features.Auth;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    /// <summary>Crée un compte utilisateur, retourne un JWT et pose le refresh token en cookie.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(request.Name, request.Surname, request.Email, request.Password, request.Birthday);
        var result = await Sender.Send(command, ct);
        RefreshTokenCookie.Append(Response, result.RefreshToken);
        return CreatedAtAction(nameof(Register), result.Response);
    }

    /// <summary>Authentifie un utilisateur, retourne un JWT et pose le refresh token en cookie.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await Sender.Send(command, ct);
        RefreshTokenCookie.Append(Response, result.RefreshToken);
        return Ok(result.Response);
    }

    /// <summary>Échange le refresh token du cookie contre un nouveau JWT et un nouveau cookie.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        try
        {
            var result = await Sender.Send(new RefreshSessionCommand(RefreshTokenCookie.Read(Request)), ct);
            RefreshTokenCookie.Append(Response, result.RefreshToken);
            return Ok(result.Response);
        }
        catch (UnauthorizedException)
        {
            // Le middleware écrit le 401 sans vider les en-têtes : la suppression du cookie
            // part avec la réponse d'erreur.
            RefreshTokenCookie.Delete(Response);
            throw;
        }
    }

    /// <summary>Révoque le refresh token du cookie, s'il existe, et supprime le cookie.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await Sender.Send(new LogoutCommand(RefreshTokenCookie.Read(Request)), ct);
        RefreshTokenCookie.Delete(Response);
        return NoContent();
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record RegisterRequest(
    string Name,
    string Surname,
    string Email,
    string Password,
    DateOnly? Birthday);

public record LoginRequest(string Email, string Password);
