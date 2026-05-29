using JewelryShop.Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Features.Auth;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    /// <summary>Crée un compte utilisateur et retourne un JWT.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command  = new RegisterCommand(request.Name, request.Surname, request.Email, request.Password, request.Birthday);
        var response = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>Authentifie un utilisateur et retourne un JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command  = new LoginCommand(request.Email, request.Password);
        var response = await Sender.Send(command, ct);
        return Ok(response);
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
