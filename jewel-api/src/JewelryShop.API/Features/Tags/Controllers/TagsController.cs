using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Queries;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Tags.Controllers;

[Route("api/tags")]
public class TagsController : ApiControllerBase
{
    /// <summary>Tags, par catégorie puis par nom (public).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Sender.Send(new GetTagsQuery(), ct);
        return Ok(result);
    }
}
