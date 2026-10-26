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
        Assert.Equal(legacy.ToPublic(), Assert.Single(page.Items, c => c.Id == legacy.Id));
        Assert.Equal(meter.ToPublic(), Assert.Single(page.Items, c => c.Id == meter.Id));
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

    [Fact]
    public async Task Place_les_chaines_favorites_de_l_utilisateur_en_tete_de_la_page_1()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));
        var favorite = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));
        var user = await _api.CreateClientAsAsync(UserRole.User);
        (await user.PostAsJsonAsync("/api/favorites", new { ChainId = favorite.Id })).EnsureSuccessStatusCode();

        var ids = await GetIdsAsync(user, "favoritesFirst=true");

        Assert.Equal(favorite.Id, ids[0]);
        Assert.Equal(ids.Skip(1).Order(), ids.Skip(1));
    }

    [Fact]
    public async Task Garde_l_ordre_habituel_des_chaines_pour_un_appel_anonyme_avec_favoritesFirst()
    {
        var anonymous = _api.CreateApiClient();

        var response = await anonymous.GetAsync($"{ChainsApi.Path}?favoritesFirst=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<ChainDto>>();
        Assert.Equal(await GetIdsAsync(anonymous, ""), page!.Items.Select(c => c.Id));
    }

    [Fact]
    public async Task Garde_l_ordre_des_chaines_par_id_sans_le_parametre_malgre_des_favoris()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var favorite = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));
        var user = await _api.CreateClientAsAsync(UserRole.User);
        (await user.PostAsJsonAsync("/api/favorites", new { ChainId = favorite.Id })).EnsureSuccessStatusCode();

        var ids = await GetIdsAsync(user, "");

        Assert.Equal(ids.Order(), ids);
    }

    private static async Task<IReadOnlyList<long>> GetIdsAsync(HttpClient client, string query)
    {
        var page = await client.GetFromJsonAsync<PagedResult<ChainDto>>($"{ChainsApi.Path}?{query}");
        return page!.Items.Select(c => c.Id).ToList();
    }

    private async Task<IReadOnlyList<ChainDto>> GetItemsAsync(string query)
    {
        var page = await _api.CreateApiClient()
            .GetFromJsonAsync<PagedResult<ChainDto>>($"{ChainsApi.Path}?pageSize=100&{query}");
        return page!.Items;
    }
}
