using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.DTOs;

namespace JewelryShop.API.IntegrationTests.Charms;

/// <summary>Le lien fournisseur et le coût d'une breloque ne sont renvoyés qu'aux admins.</summary>
[Collection(ApiCollection.Name)]
public class CharmVisibilityTests
{
    private readonly JewelryShopApiFactory _api;

    public CharmVisibilityTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Ne_renvoie_ni_lien_ni_cout_a_un_appel_anonyme()
    {
        var created = await CreateCharmAsync();
        var anonymous = _api.CreateApiClient();

        var list = await anonymous.GetAsync($"{CharmsApi.Path}?pageSize=100");
        var detail = await anonymous.GetAsync($"{CharmsApi.Path}/{created}");

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        AssertHasNoAdminFields(await FindItemAsync(list, created));
        AssertHasNoAdminFields(await detail.Content.ReadFromJsonAsync<JsonElement>());
    }

    [Fact]
    public async Task Ne_renvoie_ni_lien_ni_cout_dans_le_detail_a_un_utilisateur_connecte()
    {
        var created = await CreateCharmAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.GetAsync($"{CharmsApi.Path}/{created}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertHasNoAdminFields(await response.Content.ReadFromJsonAsync<JsonElement>());
    }

    [Fact]
    public async Task Ne_renvoie_ni_lien_ni_cout_dans_la_liste_a_un_utilisateur_connecte()
    {
        var created = await CreateCharmAsync();
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.GetAsync($"{CharmsApi.Path}?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertHasNoAdminFields(await FindItemAsync(response, created));
    }

    [Fact]
    public async Task Renvoie_le_lien_et_le_cout_dans_la_liste_et_le_detail_a_un_admin()
    {
        var created = await CreateCharmAsync();
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var list = await admin.GetAsync($"{CharmsApi.Path}?pageSize=100");
        var detail = await admin.GetAsync($"{CharmsApi.Path}/{created}");

        AssertHasAdminValues(await FindItemAsync(list, created));
        AssertHasAdminValues(await detail.Content.ReadFromJsonAsync<JsonElement>());
    }

    [Fact]
    public async Task Renvoie_le_lien_et_le_cout_a_la_creation_d_une_breloque_par_un_admin()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync(CharmsApi.Path, CharmsApi.Body(CharmsApi.UniqueName()));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        AssertHasAdminValues(await response.Content.ReadFromJsonAsync<JsonElement>());
    }

    [Fact]
    public async Task Renvoie_le_lien_et_le_cout_a_la_modification_d_une_breloque_par_un_admin()
    {
        var created = await CreateCharmAsync();
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PutAsJsonAsync($"{CharmsApi.Path}/{created}", CharmsApi.Body(CharmsApi.UniqueName()));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var charm = await response.Content.ReadFromJsonAsync<AdminCharmDto>();
        Assert.Equal(CharmsApi.SupplierUrl, charm!.Url);
        Assert.Equal(2m, charm.Cost);
    }

    private async Task<long> CreateCharmAsync()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var charm = await admin.CreateCharmAsync(CharmsApi.Body(CharmsApi.UniqueName()));
        return charm.Id;
    }

    private static async Task<JsonElement> FindItemAsync(HttpResponseMessage response, long id)
    {
        var page = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Assert.Single(page.GetProperty("items").EnumerateArray(), c => c.GetProperty("id").GetInt64() == id);
    }

    private static void AssertHasNoAdminFields(JsonElement charm)
    {
        Assert.False(charm.TryGetProperty("url", out _));
        Assert.False(charm.TryGetProperty("cost", out _));
        Assert.Equal(8m, charm.GetProperty("price").GetDecimal());
    }

    private static void AssertHasAdminValues(JsonElement charm)
    {
        Assert.Equal(CharmsApi.SupplierUrl, charm.GetProperty("url").GetString());
        Assert.Equal(2m, charm.GetProperty("cost").GetDecimal());
    }
}
