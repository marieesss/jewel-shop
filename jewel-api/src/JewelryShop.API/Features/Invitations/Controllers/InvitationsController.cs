using JewelryShop.Common;
using JewelryShop.Features.Invitations.Commands;
using JewelryShop.Features.Invitations.DTOs;
using JewelryShop.Features.Invitations.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Invitations.Controllers;

[Route("api/admin/invitations")]
[Authorize(Roles = "Admin")]
public class InvitationsController : ApiControllerBase
{
    /// <summary>Invitations paginées au statut effectif demandé (Pending par défaut), des plus récentes aux plus anciennes. [Admin]</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetInvitationsQuery(status, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Invite une personne par email : crée l'invitation et met en file le mail d'inscription. [Admin]</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateInvitationCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { InvitedById = GetUserId() }, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Renvoie une invitation non acceptée : nouveau token, 7 jours de validité et nouveau mail. [Admin]</summary>
    [HttpPost("{id:long}/resend")]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Resend(long id, CancellationToken ct)
    {
        var result = await Sender.Send(new ResendInvitationCommand(id), ct);
        return Ok(result);
    }

    /// <summary>Révoque une invitation en attente ou expirée, qui reste dans l'historique. [Admin]</summary>
    [HttpPost("{id:long}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Revoke(long id, CancellationToken ct)
    {
        await Sender.Send(new RevokeInvitationCommand(id), ct);
        return NoContent();
    }
}
