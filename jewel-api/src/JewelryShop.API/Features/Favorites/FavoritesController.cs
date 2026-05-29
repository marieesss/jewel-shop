using JewelryShop.Application.Features.Favorites.Commands;
using JewelryShop.Application.Features.Favorites.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Features.Favorites;

[Route("api/favorites")]
[Authorize]
public class FavoritesController : ApiControllerBase
{
    /// <summary>Mes favoris paginés.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyFavorites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyFavoritesQuery(GetUserId(), page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Ajoute un produit en favori (chainId OU charmId).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] AddFavoriteRequest request, CancellationToken ct)
    {
        var command = new AddFavoriteCommand(GetUserId(), request.ChainId, request.CharmId);
        var result  = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetMyFavorites), new { }, result);
    }

    /// <summary>Supprime un favori.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(long id, CancellationToken ct)
    {
        await Sender.Send(new RemoveFavoriteCommand(GetUserId(), id), ct);
        return NoContent();
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record AddFavoriteRequest(long? ChainId, long? CharmId);
