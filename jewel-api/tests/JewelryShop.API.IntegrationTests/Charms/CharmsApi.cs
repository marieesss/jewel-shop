using System.Net.Http.Json;
using JewelryShop.Common.Enums;

namespace JewelryShop.API.IntegrationTests.Charms;

public static class CharmsApi
{
    public const string Path = "/api/charms";

    /// <summary>Seul l'id est lu : l'ordre des listes ne dépend pas des autres champs du DTO.</summary>
    public record ListedCharm(long Id);

    public static async Task<long> CreateCharmAsync(this HttpClient admin, int stock = 10)
    {
        var response = await admin.PostAsJsonAsync(Path, new
        {
            Name = $"Breloque {Guid.NewGuid():N}",
            Description = "Trèfle",
            Color = ProductColor.Silver,
            Cost = 2m,
            Price = 8m,
            Stock = stock
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ListedCharm>())!.Id;
    }

    public static async Task AddFavoriteCharmAsync(this HttpClient user, long charmId)
    {
        var response = await user.PostAsJsonAsync("/api/favorites", new { CharmId = charmId });
        response.EnsureSuccessStatusCode();
    }
}
