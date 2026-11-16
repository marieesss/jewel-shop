using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.API.IntegrationTests.Orders;
using JewelryShop.Common.Enums;

namespace JewelryShop.API.IntegrationTests.Chains;

// La base est partagée entre les tests : chaque test crée ses propres chaînes et sa propre commande.
[Collection(ApiCollection.Name)]
public class DeleteChainTests
{
    private readonly JewelryShopApiFactory _api;

    public DeleteChainTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Refuse_de_supprimer_une_chaine_commandee_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = (await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()))).Id;
        await _api.SeedOrderAsync(OrdersSeed.ChainLine(chainId));

        var response = await admin.DeleteAsync($"{ChainsApi.Path}/{chainId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(OrdersSeed.ProductInOrder, error!.Title);
    }

    [Fact]
    public async Task Supprime_une_chaine_absente_des_commandes()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chainId = (await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()))).Id;

        var response = await admin.DeleteAsync($"{ChainsApi.Path}/{chainId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
