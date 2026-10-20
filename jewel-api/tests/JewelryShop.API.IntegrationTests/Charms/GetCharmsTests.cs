using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common;
using JewelryShop.Common.Enums;

namespace JewelryShop.API.IntegrationTests.Charms;

[Collection(ApiCollection.Name)]
public class GetCharmsTests
{
    private readonly JewelryShopApiFactory _api;

    public GetCharmsTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Place_les_favoris_de_l_utilisateur_en_tete_de_la_page_1()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var charmIds = new List<long>();
        for (var i = 0; i < 30; i++)
            charmIds.Add(await admin.CreateCharmAsync());
        var user = await _api.CreateClientAsAsync(UserRole.User);
        await user.AddFavoriteCharmAsync(charmIds[25]);
        await user.AddFavoriteCharmAsync(charmIds[10]);

        var ids = await GetIdsAsync(user, "favoritesFirst=true");

        Assert.Equal([charmIds[10], charmIds[25]], ids.Take(2));
        Assert.Equal(ids.Skip(2).Order(), ids.Skip(2));
    }

    [Fact]
    public async Task Garde_l_ordre_habituel_pour_un_appel_anonyme_avec_favoritesFirst()
    {
        var anonymous = _api.CreateApiClient();

        var response = await anonymous.GetAsync($"{CharmsApi.Path}?favoritesFirst=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<CharmsApi.ListedCharm>>();
        Assert.Equal(await GetIdsAsync(anonymous, ""), page!.Items.Select(c => c.Id));
    }

    [Fact]
    public async Task Garde_l_ordre_par_id_sans_le_parametre_malgre_des_favoris()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var favoriteId = await admin.CreateCharmAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);
        await user.AddFavoriteCharmAsync(favoriteId);

        var ids = await GetIdsAsync(user, "");

        Assert.Equal(ids.Order(), ids);
        Assert.Equal(await GetIdsAsync(_api.CreateApiClient(), ""), ids);
    }

    [Fact]
    public async Task Applique_les_filtres_avant_de_placer_les_favoris_en_premier()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var outOfStockId = await admin.CreateCharmAsync(stock: 0);
        var inStockId = await admin.CreateCharmAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);
        await user.AddFavoriteCharmAsync(outOfStockId);
        await user.AddFavoriteCharmAsync(inStockId);

        var ids = await GetIdsAsync(user, "favoritesFirst=true&inStockOnly=true");

        Assert.Equal(inStockId, ids[0]);
        Assert.DoesNotContain(outOfStockId, ids);
    }

    private static async Task<IReadOnlyList<long>> GetIdsAsync(HttpClient client, string query)
    {
        var page = await client.GetFromJsonAsync<PagedResult<CharmsApi.ListedCharm>>($"{CharmsApi.Path}?{query}");
        return page!.Items.Select(c => c.Id).ToList();
    }
}
