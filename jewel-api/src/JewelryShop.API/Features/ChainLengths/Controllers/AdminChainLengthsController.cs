using JewelryShop.Features.ChainLengths.Commands;
using JewelryShop.Features.ChainLengths.DTOs;
using JewelryShop.Features.ChainLengths.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.ChainLengths.Controllers;

[Route("api/admin/chain-lengths")]
[Authorize(Roles = "Admin")]
public class AdminChainLengthsController : ApiControllerBase
{
    /// <summary>Longueurs de chaîne, de la plus courte à la plus longue. [Admin]</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ChainLengthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Sender.Send(new GetChainLengthsQuery(), ct);
        return Ok(result);
    }

    /// <summary>Ajoute une longueur de chaîne. [Admin]</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChainLengthDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateChainLengthCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Modifie une longueur de chaîne. [Admin]</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ChainLengthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateChainLengthCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { Id = id }, ct);
        return Ok(result);
    }

    /// <summary>Supprime une longueur de chaîne. [Admin]</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await Sender.Send(new DeleteChainLengthCommand(id), ct);
        return NoContent();
    }
}
