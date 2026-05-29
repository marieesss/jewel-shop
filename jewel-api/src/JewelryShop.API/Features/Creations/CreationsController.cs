using JewelryShop.Application.Features.Creations.Commands;
using JewelryShop.Application.Features.Creations.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Features.Creations;

[Route("api/creations")]
[Authorize]
public class CreationsController : ApiControllerBase
{
    /// <summary>Mes créations paginées.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCreations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyCreationsQuery(GetUserId(), page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Détail d'une de mes créations.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetCreationByIdQuery(GetUserId(), id), ct);
        return Ok(result);
    }

    /// <summary>Crée une nouvelle création personnalisée.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateCreationRequest request, CancellationToken ct)
    {
        var items   = request.Items.Select(i => new CreateCreationItemInput(i.CharmId, i.Position)).ToList();
        var command = new CreateCreationCommand(GetUserId(), request.ChainId, request.OptionId, items);
        var result  = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Supprime une de mes créations.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await Sender.Send(new DeleteCreationCommand(GetUserId(), id), ct);
        return NoContent();
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record CreationItemRequest(long CharmId, int Position);

public record CreateCreationRequest(
    long ChainId,
    long OptionId,
    IReadOnlyList<CreationItemRequest> Items);
