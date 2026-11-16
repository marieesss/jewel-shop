using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.DTOs;

namespace JewelryShop.API.IntegrationTests.Orders;

public static class OrdersApi
{
    public const string QuotePath = "/api/orders/quote";

    /// <summary>Longueur créée par la migration V8 : elle existe dans toute base de test.</summary>
    public const int OfferedLengthCm = 45;

    public static object QuoteBody(long chainId, params long[] charmIds) => QuoteBody(chainId, OfferedLengthCm, charmIds);

    public static object QuoteBody(long chainId, int lengthCm, long[] charmIds) => new { chainId, lengthCm, charmIds };

    /// <summary>Chaîne à 10 € le mètre avec un fermoir à 2 €, sur un seul rouleau.</summary>
    public static async Task<long> CreateQuoteChainAsync(this HttpClient admin, int rollLengthCm = 250) =>
        (await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName(), claspPrice: 2m, initialRollLengthCm: rollLengthCm))).Id;

    /// <summary>Breloque à 5 €.</summary>
    public static Task<AdminCharmDto> CreateQuoteCharmAsync(this HttpClient admin, int stock = 10) =>
        admin.CreateCharmAsync(new
        {
            Name = CharmsApi.UniqueName(),
            Description = "Trèfle",
            Color = ProductColor.Silver,
            Cost = 2m,
            Price = 5m,
            Stock = stock
        });

    public static async Task<int> GetCharmStockAsync(this HttpClient client, long charmId)
    {
        var response = await client.GetAsync($"{CharmsApi.Path}/{charmId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CharmDto>())!.Stock;
    }
}
