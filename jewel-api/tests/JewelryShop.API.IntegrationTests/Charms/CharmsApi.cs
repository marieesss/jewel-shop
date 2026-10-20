using System.Net.Http.Json;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.DTOs;

namespace JewelryShop.API.IntegrationTests.Charms;

public static class CharmsApi
{
    public const string Path = "/api/charms";

    public const string SupplierUrl = "https://fournisseur.example/breloque";

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

    public static async Task<AdminCharmDto> CreateCharmAsync(this HttpClient admin, object body)
    {
        var response = await admin.PostAsJsonAsync(Path, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AdminCharmDto>())!;
    }

    public static string UniqueName() => $"Breloque {Guid.NewGuid():N}";
}
