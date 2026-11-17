using JewelryShop.Common.Models;
using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Orders.Controllers;

[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ApiControllerBase
{
    public const string Path = "/api/admin/orders";

    /// <summary>
    /// Commandes de tous les clients, des plus récentes aux plus anciennes, filtrables par statut
    /// (« Pending », « Shipped »…). [Admin]
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminOrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = PaginationDefaults.DefaultPage,
        [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetOrdersQuery(status, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Détail d'une commande avec le client, l'historique complet et les notes internes. [Admin]</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(AdminOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetOrderByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Ajoute une note interne à la commande, signée par l'admin connecté. [Admin]</summary>
    [HttpPost("{id:long}/notes")]
    [ProducesResponseType(typeof(OrderNoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(long id, [FromBody] AddOrderNoteCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { OrderId = id, AuthorId = GetUserId() }, ct);
        // L'URL relative est construite à la main : CreatedAtAction produirait une URL absolue.
        return Created($"{Path}/{id}/notes/{result.Id}", result);
    }

    /// <summary>Corrige le contenu d'une note interne ; son auteur ne change pas. [Admin]</summary>
    [HttpPut("{id:long}/notes/{noteId:long}")]
    [ProducesResponseType(typeof(OrderNoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNote(long id, long noteId, [FromBody] UpdateOrderNoteCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { OrderId = id, NoteId = noteId }, ct);
        return Ok(result);
    }
}
