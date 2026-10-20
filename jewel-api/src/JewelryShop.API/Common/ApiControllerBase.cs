using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;

    /// <summary>Injecté via la propriété pour ne pas imposer de constructeur sur chaque controller.</summary>
    protected ISender Sender =>
        _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Extrait l'Id utilisateur depuis le claim "sub" du JWT.
    /// Lève une InvalidOperationException si le claim est absent (ne devrait jamais arriver
    /// sur un endpoint [Authorize]).
    /// </summary>
    protected long GetUserId()
        => GetUserIdOrNull()
           ?? throw new InvalidOperationException("Claim 'sub' introuvable dans le token.");

    /// <summary>
    /// Id de l'utilisateur connecté, ou null pour un appel anonyme (endpoint sans [Authorize]
    /// appelé sans token, ou avec un token invalide ou expiré).
    /// </summary>
    protected long? GetUserIdOrNull()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");

        return long.TryParse(value, out var id) ? id : null;
    }
}
