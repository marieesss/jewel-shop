using System.Net.Http.Headers;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Auth.DTOs;
using JewelryShop.Features.Chains.DTOs;

namespace JewelryShop.API.IntegrationTests.Chains;

public static class ChainsApi
{
    public const string Path = "/api/chains";

    public const string SupplierUrl = "https://fournisseur.example/chaine";

    /// <summary>Crée un compte du rôle demandé, s'y connecte et renvoie un client qui envoie son JWT.</summary>
    public static async Task<HttpClient> CreateClientAsAsync(this JewelryShopApiFactory api, UserRole role)
    {
        var user = await api.CreateUserAsync(role);
        var client = api.CreateApiClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { user.Email, Password = TestAccounts.Password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    /// <summary>Corps envoyé par le front en production : prix et coût totaux pour une longueur.</summary>
    public static object LegacyBody(string name, decimal cost = 20m, decimal price = 50m, decimal length = 500m) => new
    {
        Name = name,
        Description = "Maille fine",
        Color = ProductColor.Gold,
        Cost = cost,
        Price = price,
        Length = length
    };

    public static object MeterBody(
        string name, ProductColor color = ProductColor.Gold, decimal claspPrice = 3m, int initialRollLengthCm = 250) => new
        {
            Name = name,
            Description = "Maille fine",
            Color = color,
            CostPerMeter = 4m,
            PricePerMeter = 10m,
            ClaspCost = 1.5m,
            ClaspPrice = claspPrice,
            InitialRollLengthCm = initialRollLengthCm,
            Url = SupplierUrl
        };

    public static async Task<AdminChainDto> CreateChainAsync(this HttpClient admin, object body)
    {
        var response = await admin.PostAsJsonAsync(Path, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AdminChainDto>())!;
    }

    /// <summary>La même chaîne telle qu'un client la reçoit, sans lien ni coûts.</summary>
    public static ChainDto ToPublic(this AdminChainDto c) => new(
        c.Id, c.Name, c.Description, c.Color, c.ImageUrl, c.Price, c.Length, c.PricePerMeter, c.ClaspPrice, c.StockCm,
        c.AvailableLengths, c.FromPrice);

    public static string UniqueName() => $"Chaîne {Guid.NewGuid():N}";
}
