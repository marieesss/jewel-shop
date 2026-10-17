using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Invitations.DTOs;
using JewelryShop.Features.Invitations.Queries;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Auth.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    /// <summary>Crée un compte utilisateur, retourne un JWT et pose le refresh token en cookie.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        RefreshTokenCookie.Append(Response, result.RefreshToken);
        return CreatedAtAction(nameof(Register), result.Response);
    }

    /// <summary>
    /// Renvoie l'email et l'échéance de l'invitation en attente qui porte ce token, pour la page d'inscription.
    /// Répond 404 si l'invitation est inconnue, expirée, révoquée ou déjà acceptée.
    /// </summary>
    [HttpGet("invitations/{token}")]
    [ProducesResponseType(typeof(InvitationPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvitation(string token, CancellationToken ct)
        => Ok(await Sender.Send(new GetInvitationByTokenQuery(token), ct));

    /// <summary>Authentifie un utilisateur, retourne un JWT et pose le refresh token en cookie.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
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

    /// <summary>
    /// Envoie un lien de réinitialisation du mot de passe si un compte existe pour cet email.
    /// Répond 202 dans tous les cas, pour ne pas révéler les emails inscrits.
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        await Sender.Send(command, ct);
        return Accepted();
    }

    /// <summary>
    /// Remplace le mot de passe avec le token reçu par mail, puis ferme toutes les sessions ouvertes.
    /// Répond 400 si le token est invalide, déjà utilisé ou expiré.
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        await Sender.Send(command, ct);
        return NoContent();
    }
}
