using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Chains.Controllers;

[Route("api/chains")]
public class ChainsController : ApiControllerBase
{
    /// <summary>Liste paginée avec filtres optionnels.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? color,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        ProductColor? colorEnum = Enum.TryParse<ProductColor>(color, ignoreCase: true, out var c) ? c : null;
        var result = await Sender.Send(new GetChainsQuery(colorEnum, minPrice, maxPrice, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Détail d'une chaîne.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetChainByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Crée une chaîne. [Admin]</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateChainCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Met à jour une chaîne. [Admin]</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateChainCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { Id = id }, ct);
        return Ok(result);
    }

    /// <summary>Supprime une chaîne. [Admin]</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await Sender.Send(new DeleteChainCommand(id), ct);
        return NoContent();
    }

    /// <summary>Upload de l'image d'une chaîne. [Admin]</summary>
    [HttpPost("{id:long}/image")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(long id, IFormFile? file, CancellationToken ct)
    {
        var command = new UploadChainImageCommand(
            id,
            file?.OpenReadStream() ?? Stream.Null,
            file?.Length ?? 0,
            file?.FileName ?? string.Empty,
            file?.ContentType ?? string.Empty);
        var imageUrl = await Sender.Send(command, ct);
        return Ok(new { imageUrl });
    }
}
