using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Tags.Controllers;

[Route("api/admin/tags")]
[Authorize(Roles = "Admin")]
public class AdminTagsController : ApiControllerBase
{
    /// <summary>Tags, par catégorie puis par nom, avec le nombre de chaînes et de breloques qui les portent. [Admin]</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminTagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Sender.Send(new GetAdminTagsQuery(), ct);
        return Ok(result);
    }
}
