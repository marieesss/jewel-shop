using JewelryShop.Features.Creations.Commands;
using JewelryShop.Features.Creations.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Creations.Controllers;

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
    public async Task<IActionResult> Create([FromBody] CreateCreationCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { UserId = GetUserId() }, ct);
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
