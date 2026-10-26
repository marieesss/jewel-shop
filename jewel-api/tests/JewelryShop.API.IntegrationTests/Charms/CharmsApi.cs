using System.Net.Http.Json;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.DTOs;

namespace JewelryShop.API.IntegrationTests.Charms;

public static class CharmsApi
{
    public const string Path = "/api/charms";

    public const string SupplierUrl = "https://fournisseur.example/breloque";

    /// <summary>Seul l'id est lu : l'ordre des listes ne dépend pas des autres champs du DTO.</summary>
    public record ListedCharm(long Id);

    public static object Body(string name) => new
    {
        Name = name,
        Description = "Argent massif",
        Color = ProductColor.Silver,
        Cost = 2m,
        Price = 8m,
        Stock = 10,
        Url = SupplierUrl
    };

    public static string UniqueName() => $"Breloque {Guid.NewGuid():N}";

    public static async Task<AdminCharmDto> CreateCharmAsync(this HttpClient admin, object body)
    {
        var response = await admin.PostAsJsonAsync(Path, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AdminCharmDto>())!;
    }

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
