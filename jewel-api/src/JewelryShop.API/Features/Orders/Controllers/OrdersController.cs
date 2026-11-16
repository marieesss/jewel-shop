using JewelryShop.Features.Orders.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Orders.Controllers;

[Route("api/orders")]
[Authorize]
public class OrdersController : ApiControllerBase
{
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
}
