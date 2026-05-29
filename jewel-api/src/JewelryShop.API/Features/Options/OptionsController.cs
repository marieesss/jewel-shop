using JewelryShop.Application.Features.Options.Queries;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Features.Options;

[Route("api/options")]
public class OptionsController : ApiControllerBase
{
    /// <summary>Liste de toutes les options disponibles (public).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Sender.Send(new GetOptionsQuery(), ct);
        return Ok(result);
    }
}
