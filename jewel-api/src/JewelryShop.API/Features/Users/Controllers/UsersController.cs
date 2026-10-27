using JewelryShop.Features.Users.DTOs;
using JewelryShop.Features.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Features.Users.Controllers;

[Route("api/users")]
[Authorize]
public class UsersController : ApiControllerBase
{
    /// <summary>Profil de l'utilisateur connecté, avec son adresse de livraison et son téléphone.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await Sender.Send(new GetMyProfileQuery(GetUserId()), ct);
        return Ok(result);
    }
}
