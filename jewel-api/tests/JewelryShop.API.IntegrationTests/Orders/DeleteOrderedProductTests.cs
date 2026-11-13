using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.IntegrationTests.Orders;

// La base est partagée entre les tests : chaque test crée ses propres articles et sa propre commande.
[Collection(ApiCollection.Name)]
public class DeleteOrderedProductTests
{
    private const string ProductInOrder = "Ce produit figure dans une commande.";

    private readonly JewelryShopApiFactory _api;

    public DeleteOrderedProductTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Refuse_de_supprimer_une_breloque_commandee_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var charmId = await admin.CreateCharmAsync();
        await SeedOrderAsync(new OrderLine { Kind = OrderLineKind.Charm, CharmId = charmId, Name = "Breloque", UnitPrice = 4.5m, Position = 1 });

        var response = await admin.DeleteAsync($"{CharmsApi.Path}/{charmId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(ProductInOrder, error!.Title);
    }

    [Fact]
    public async Task Refuse_de_supprimer_une_chaine_commandee_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = (await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()))).Id;
        await SeedOrderAsync(new OrderLine
        {
            Kind = OrderLineKind.Chain,
            ChainId = chainId,
            Name = "Chaîne",
            LengthCm = 45,
            PricePerMeter = 20m,
            ClaspPrice = 3m,
            UnitPrice = 12m
        });

        var response = await admin.DeleteAsync($"{ChainsApi.Path}/{chainId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(ProductInOrder, error!.Title);
    }

    [Fact]
    public async Task Supprime_toujours_un_article_absent_des_commandes()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var charmId = await admin.CreateCharmAsync();
        var chainId = (await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()))).Id;

        var charmResponse = await admin.DeleteAsync($"{CharmsApi.Path}/{charmId}");
        var chainResponse = await admin.DeleteAsync($"{ChainsApi.Path}/{chainId}");

        Assert.Equal(HttpStatusCode.NoContent, charmResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, chainResponse.StatusCode);
    }

    private async Task SeedOrderAsync(OrderLine line)
    {
        var customer = await _api.CreateUserAsync();
        await _api.SeedAsync(async services =>
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Orders.Add(new Order
            {
                Number = $"CMD-T-{Guid.NewGuid():N}"[..20],
                UserId = customer.Id,
                RecipientName = "Martin",
                RecipientSurname = "Alice",
                RecipientPhone = "0600000000",
                AddressLine1 = "1 rue des Lilas",
                PostalCode = "75001",
                City = "Paris",
                Total = line.UnitPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Lines = { line }
            });
            await context.SaveChangesAsync();
        });
    }
}
