using JewelryShop.Application.Features.Chains.Commands;
using JewelryShop.Application.Features.Chains.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Features.Chains;

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
    public async Task<IActionResult> Create([FromBody] CreateChainRequest request, CancellationToken ct)
    {
        var command = new CreateChainCommand(
            request.Name, request.Description, request.Color,
            request.Cost, request.Price, request.Length, request.Url);
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Met à jour une chaîne. [Admin]</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateChainRequest request, CancellationToken ct)
    {
        var command = new UpdateChainCommand(
            id, request.Name, request.Description, request.Color,
            request.Cost, request.Price, request.Length, request.Url);
        var result = await Sender.Send(command, ct);
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
    public async Task<IActionResult> UploadImage(long id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Aucun fichier fourni.");

        var command  = new UploadChainImageCommand(id, file.OpenReadStream(), file.FileName, file.ContentType);
        var imageUrl = await Sender.Send(command, ct);
        return Ok(new { imageUrl });
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateChainRequest(
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    decimal Length,
    string? Url = null);

public record UpdateChainRequest(
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    decimal Length,
    string? Url = null);
