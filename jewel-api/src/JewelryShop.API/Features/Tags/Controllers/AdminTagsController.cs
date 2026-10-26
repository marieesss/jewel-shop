using JewelryShop.Features.Tags.Commands;
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

    /// <summary>Crée un tag dans une catégorie (« Theme », « Color », « Material » ou « Occasion »). [Admin]</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTagCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Corrige le nom d'un tag ; son slug est recalculé. [Admin]</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rename(long id, [FromBody] RenameTagCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { Id = id }, ct);
        return Ok(result);
    }

    /// <summary>Supprime un tag et ses associations ; les chaînes et les breloques restent. [Admin]</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await Sender.Send(new DeleteTagCommand(id), ct);
        return NoContent();
    }
}
