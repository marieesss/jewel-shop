using JewelryShop.Features.ChainLengths.DTOs;
using JewelryShop.Features.ChainLengths.Queries;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.ChainLengths.Controllers;

[Route("api/chain-lengths")]
public class ChainLengthsController : ApiControllerBase
{
    /// <summary>Longueurs de chaîne proposées, de la plus courte à la plus longue (public).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ChainLengthDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Sender.Send(new GetChainLengthsQuery(), ct);
        return Ok(result);
    }
}
