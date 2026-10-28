using JewelryShop.Features.Tags.Queries;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Tags.Controllers;

[Route("api/tags")]
public class TagsController : ApiControllerBase
{
    /// <summary>
    /// Tags, par catégorie puis par nom (public). Avec <c>kind=chain</c> ou <c>kind=charm</c>, chaque tag a aussi
    /// <c>count</c> : le nombre global de chaînes ou de breloques qui le portent, sans tenir compte des filtres du catalogue.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] string? kind, CancellationToken ct)
    {
        object result = kind is null
            ? await Sender.Send(new GetTagsQuery(), ct)
            : await Sender.Send(new GetTagCountsQuery(kind), ct);
        return Ok(result);
    }
}
