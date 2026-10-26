using JewelryShop.Features.Charms.Commands;
using JewelryShop.Features.Charms.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Charms.Controllers;

[Route("api/charms")]
public class CharmsController : ApiControllerBase
{
    /// <summary>
    /// Liste paginée avec filtres optionnels. <c>favoritesFirst=true</c> place d'abord les favoris
    /// de l'utilisateur connecté ; il est ignoré pour un appel anonyme.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? color,
        [FromQuery] bool? inStockOnly,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool favoritesFirst = false,
        CancellationToken ct = default)
    {
        ProductColor? colorEnum = Enum.TryParse<ProductColor>(color, ignoreCase: true, out var c) ? c : null;
        var favoritesFirstForUserId = favoritesFirst ? GetUserIdOrNull() : null;
        var query = new GetCharmsQuery(
            colorEnum, inStockOnly, minPrice, maxPrice, page, pageSize, favoritesFirstForUserId);
        var result = await Sender.Send(query, ct);
        return Ok(result);
    }

    /// <summary>Détail d'une breloque.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetCharmByIdQuery(id, IsAdmin), ct);
        return Ok(result);
    }

    /// <summary>Crée une breloque. [Admin]</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCharmCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Met à jour une breloque. [Admin]</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCharmCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { Id = id }, ct);
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(long id, IFormFile? file, CancellationToken ct)
    {
        var command = new UploadCharmImageCommand(
            id,
            file?.OpenReadStream() ?? Stream.Null,
            file?.Length ?? 0,
            file?.FileName ?? string.Empty,
            file?.ContentType ?? string.Empty);
        var imageUrl = await Sender.Send(command, ct);
        return Ok(new { imageUrl });
    }
}
