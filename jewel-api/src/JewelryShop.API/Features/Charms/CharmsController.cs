using JewelryShop.Application.Features.Charms.Commands;
using JewelryShop.Application.Features.Charms.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Features.Charms;

[Route("api/charms")]
public class CharmsController : ApiControllerBase
{
    /// <summary>Liste paginée avec filtres optionnels.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? color,
        [FromQuery] bool? inStockOnly,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        ProductColor? colorEnum = Enum.TryParse<ProductColor>(color, ignoreCase: true, out var c) ? c : null;
        var result = await Sender.Send(
            new GetCharmsQuery(colorEnum, inStockOnly, minPrice, maxPrice, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Détail d'une breloque.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetCharmByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Crée une breloque. [Admin]</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCharmRequest request, CancellationToken ct)
    {
        var command = new CreateCharmCommand(
            request.Name, request.Description, request.Color,
            request.Cost, request.Price, request.Stock);
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Met à jour une breloque. [Admin]</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCharmRequest request, CancellationToken ct)
    {
        var command = new UpdateCharmCommand(
            id, request.Name, request.Description, request.Color,
            request.Cost, request.Price, request.Stock);
        var result = await Sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Supprime une breloque. [Admin]</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await Sender.Send(new DeleteCharmCommand(id), ct);
        return NoContent();
    }

    /// <summary>Upload de l'image d'une breloque. [Admin]</summary>
    [HttpPost("{id:long}/image")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(long id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Aucun fichier fourni.");

        var command  = new UploadCharmImageCommand(id, file.OpenReadStream(), file.FileName, file.ContentType);
        var imageUrl = await Sender.Send(command, ct);
        return Ok(new { imageUrl });
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateCharmRequest(
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    int Stock);

public record UpdateCharmRequest(
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    int Stock);
