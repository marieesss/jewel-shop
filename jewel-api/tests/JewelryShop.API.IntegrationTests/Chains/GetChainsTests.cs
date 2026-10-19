using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.DTOs;

namespace JewelryShop.API.IntegrationTests.Chains;

[Collection(ApiCollection.Name)]
public class GetChainsTests
{
    private readonly JewelryShopApiFactory _api;

    public GetChainsTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Liste_les_chaines_des_deux_formats_sans_connexion()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var legacy = await admin.CreateChainAsync(ChainsApi.LegacyBody(ChainsApi.UniqueName()));
        var meter = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));

        var response = await _api.CreateApiClient().GetAsync($"{ChainsApi.Path}?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<ChainDto>>();
        Assert.NotNull(page);
        Assert.Equal(legacy, Assert.Single(page.Items, c => c.Id == legacy.Id));
        Assert.Equal(meter, Assert.Single(page.Items, c => c.Id == meter.Id));
    }

    [Fact]
    public async Task Renvoie_le_stock_et_les_prix_au_metre_dans_le_detail_sans_connexion()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var created = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));

        var response = await _api.CreateApiClient().GetAsync($"{ChainsApi.Path}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var chain = await response.Content.ReadFromJsonAsync<ChainDto>();
        Assert.NotNull(chain);
        Assert.Equal(250, chain.StockCm);
        Assert.Equal(10m, chain.PricePerMeter);
        Assert.Null(chain.Price);
    }
}
