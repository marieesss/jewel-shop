using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.API.IntegrationTests.Users;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.DTOs;

namespace JewelryShop.API.IntegrationTests.Orders;

public static class OrdersApi
{
    public const string Path = "/api/orders";
    public const string QuotePath = "/api/orders/quote";

    /// <summary>Longueur créée par la migration V8 : elle existe dans toute base de test.</summary>
    public const int OfferedLengthCm = 45;

    public static object QuoteBody(long chainId, params long[] charmIds) => QuoteBody(chainId, OfferedLengthCm, charmIds);

    public static object QuoteBody(long chainId, int lengthCm, long[] charmIds) => new { chainId, lengthCm, charmIds };

    /// <summary>Commande livrée au titulaire du compte, à l'adresse de son profil.</summary>
    public static object OrderBody(long chainId, params long[] charmIds) => new { chainId, lengthCm = OfferedLengthCm, charmIds };

    /// <summary>Profil complet, envoyé tel quel à <c>PUT /api/users/me/address</c>.</summary>
    public static readonly object ProfileAddress = new
    {
        Line1 = "12 rue des Lilas",
        Line2 = (string?)null,
        PostalCode = "75011",
        City = "Paris",
        Phone = "0612345678"
    };

    /// <summary>Client d'un compte qui a enregistré son adresse et son téléphone : il peut commander sans les saisir.</summary>
    public static async Task<HttpClient> CreateCustomerClientAsync(this JewelryShopApiFactory api)
    {
        var client = await api.CreateClientAsAsync(UserRole.User);
        var response = await client.PutAsJsonAsync(UsersApi.MyAddressPath, ProfileAddress);
        response.EnsureSuccessStatusCode();
        return client;
    }

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
