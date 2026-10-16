using JewelryShop.Features.Invitations.Commands;
using JewelryShop.Features.Invitations.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Invitations.Controllers;

[Route("api/admin/invitations")]
[Authorize(Roles = "Admin")]
public class InvitationsController : ApiControllerBase
{
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
}
