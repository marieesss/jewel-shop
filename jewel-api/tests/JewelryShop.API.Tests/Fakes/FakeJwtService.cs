using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Application.Features.Users;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>Renvoie un JWT différent à chaque appel, pour vérifier qu'un nouveau token est émis.</summary>
public class FakeJwtService : IJwtService
{
    public int GeneratedCount { get; private set; }

    public string GenerateToken(User user) => $"jwt-{user.Id}-{++GeneratedCount}";
}
