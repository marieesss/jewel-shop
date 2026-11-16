using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.API.IntegrationTests.Orders;
using JewelryShop.Common.Enums;

namespace JewelryShop.API.IntegrationTests.Charms;

// La base est partagée entre les tests : chaque test crée ses propres breloques et sa propre commande.
[Collection(ApiCollection.Name)]
public class DeleteCharmTests
{
    private readonly JewelryShopApiFactory _api;

    public DeleteCharmTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Refuse_de_supprimer_une_breloque_commandee_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var charmId = await admin.CreateCharmAsync();
        await _api.SeedOrderAsync(OrdersSeed.CharmLine(charmId));

        var response = await admin.DeleteAsync($"{CharmsApi.Path}/{charmId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(OrdersSeed.ProductInOrder, error!.Title);
    }

    [Fact]
    public async Task Supprime_une_breloque_absente_des_commandes()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var charmId = await admin.CreateCharmAsync();

        var response = await admin.DeleteAsync($"{CharmsApi.Path}/{charmId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
