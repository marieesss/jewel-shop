using JewelryShop.Common.Models;
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

    /// <summary>Commandes de l'utilisateur connecté, des plus récentes aux plus anciennes.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int page = PaginationDefaults.DefaultPage,
        [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetMyOrdersQuery(GetUserId(), page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Détail d'une commande de l'utilisateur connecté. Celle d'un autre utilisateur est introuvable.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetMyOrderByIdQuery(GetUserId(), id), ct);
        return Ok(result);
    }

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
        // L'URL relative est construite à la main : CreatedAtAction produirait une URL absolue.
        return Created($"{Path}/{result.Id}", result);
    }

    /// <summary>Annule une commande en validation de l'utilisateur connecté et recrédite son stock.</summary>
    [HttpPost("{id:long}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(long id, CancellationToken ct)
    {
        var result = await Sender.Send(new CancelOrderCommand(GetUserId(), id), ct);
        return Ok(result);
    }

    /// <summary>Confirme la réception d'une commande envoyée de l'utilisateur connecté.</summary>
    [HttpPost("{id:long}/receive")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Receive(long id, CancellationToken ct)
    {
        var result = await Sender.Send(new ReceiveOrderCommand(GetUserId(), id), ct);
        return Ok(result);
    }
}
