using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.DTOs;
using JewelryShop.Features.Favorites.DTOs;

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

    [Fact]
    public async Task Filtre_par_prix_les_chaines_du_nouveau_format_sur_leur_prix_au_metre()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var meter = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));

        var inRange = await GetItemsAsync("minPrice=10&maxPrice=10");
        var tooExpensive = await GetItemsAsync("maxPrice=9.99");

        Assert.Contains(inRange, c => c.Id == meter.Id);
        Assert.DoesNotContain(tooExpensive, c => c.Id == meter.Id);
    }

    [Fact]
    public async Task Renvoie_le_stock_de_la_chaine_dans_les_favoris()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var created = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));

        var response = await admin.PostAsJsonAsync("/api/favorites", new { ChainId = created.Id });
        response.EnsureSuccessStatusCode();

        var favorites = await admin.GetFromJsonAsync<PagedResult<FavoriteDto>>("/api/favorites?pageSize=100");
        var favorite = Assert.Single(favorites!.Items, f => f.ChainId == created.Id);
        Assert.Equal(250, favorite.Chain!.StockCm);
    }

    private async Task<IReadOnlyList<ChainDto>> GetItemsAsync(string query)
    {
        var page = await _api.CreateApiClient()
            .GetFromJsonAsync<PagedResult<ChainDto>>($"{ChainsApi.Path}?pageSize=100&{query}");
        return page!.Items;
    }
}
