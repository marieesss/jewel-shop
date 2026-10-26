using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;

namespace JewelryShop.API.IntegrationTests.Chains;

/// <summary>Le lien fournisseur et les coûts d'une chaîne ne sont renvoyés qu'aux admins.</summary>
[Collection(ApiCollection.Name)]
public class ChainVisibilityTests
{
    private static readonly string[] _adminFields = ["url", "cost", "costPerMeter", "claspCost"];

    private readonly JewelryShopApiFactory _api;

    public ChainVisibilityTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Ne_renvoie_ni_lien_ni_couts_dans_la_liste_a_un_appel_anonyme()
    {
        var created = await CreateChainAsync();

        var response = await _api.CreateApiClient().GetAsync($"{ChainsApi.Path}?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertHasNoAdminFields(await FindItemAsync(response, created));
    }

    [Fact]
    public async Task Ne_renvoie_ni_lien_ni_couts_dans_le_detail_a_un_appel_anonyme()
    {
        var created = await CreateChainAsync();

        var response = await _api.CreateApiClient().GetAsync($"{ChainsApi.Path}/{created}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertHasNoAdminFields(await response.Content.ReadFromJsonAsync<JsonElement>());
    }

    [Fact]
    public async Task Ne_renvoie_ni_lien_ni_couts_a_un_utilisateur_connecte()
    {
        var created = await CreateChainAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var list = await user.GetAsync($"{ChainsApi.Path}?pageSize=100");
        var detail = await user.GetAsync($"{ChainsApi.Path}/{created}");

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        AssertHasNoAdminFields(await FindItemAsync(list, created));
        AssertHasNoAdminFields(await detail.Content.ReadFromJsonAsync<JsonElement>());
    }

    [Fact]
    public async Task Renvoie_le_lien_et_les_couts_de_chaque_chaine_de_la_liste_a_un_admin()
    {
        var created = await CreateChainAsync();
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.GetAsync($"{ChainsApi.Path}?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.All(page.GetProperty("items").EnumerateArray(), AssertHasAdminFields);
        AssertHasAdminValues(FindItem(page, created));
    }

    [Fact]
    public async Task Renvoie_le_lien_et_les_couts_dans_le_detail_a_un_admin()
    {
        var created = await CreateChainAsync();
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.GetAsync($"{ChainsApi.Path}/{created}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertHasAdminValues(await response.Content.ReadFromJsonAsync<JsonElement>());
    }

    private async Task<long> CreateChainAsync()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var chain = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));
        return chain.Id;
    }

    private static async Task<JsonElement> FindItemAsync(HttpResponseMessage response, long id) =>
        FindItem(await response.Content.ReadFromJsonAsync<JsonElement>(), id);

    private static JsonElement FindItem(JsonElement page, long id) =>
        Assert.Single(page.GetProperty("items").EnumerateArray(), c => c.GetProperty("id").GetInt64() == id);

    private static void AssertHasNoAdminFields(JsonElement chain)
    {
        Assert.All(_adminFields, field => Assert.False(chain.TryGetProperty(field, out _), field));
        Assert.Equal(10m, chain.GetProperty("pricePerMeter").GetDecimal());
    }

    private static void AssertHasAdminFields(JsonElement chain) =>
        Assert.All(_adminFields, field => Assert.True(chain.TryGetProperty(field, out _), field));

    // Chaîne créée au nouveau format : le coût total de l'ancien format est renvoyé, mais vide.
    private static void AssertHasAdminValues(JsonElement chain)
    {
        Assert.Equal(ChainsApi.SupplierUrl, chain.GetProperty("url").GetString());
        Assert.Equal(4m, chain.GetProperty("costPerMeter").GetDecimal());
        Assert.Equal(1.5m, chain.GetProperty("claspCost").GetDecimal());
        Assert.Equal(JsonValueKind.Null, chain.GetProperty("cost").ValueKind);
    }
}
