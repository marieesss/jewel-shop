using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Orders.Controllers;

[Route("api/orders")]
[Authorize]
public class OrdersController : ApiControllerBase
{
    public const string Path = "/api/orders";

    /// <summary>Lignes et total d'un collier, calculés sans rien enregistrer.</summary>
    [HttpPost("quote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Quote([FromBody] GetQuoteQuery query, CancellationToken ct)
    {
        var result = await Sender.Send(query, ct);
        return Ok(result);
    }

    /// <summary>Commande un collier pour l'utilisateur connecté et prélève son stock.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { UserId = GetUserId() }, ct);
        // Aucune action ne lit encore une commande : CreatedAtAction n'a pas de cible, l'URL est construite à la main.
        return Created($"{Path}/{result.Id}", result);
    }
}
