using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Features.Auth.DTOs;

namespace JewelryShop.API.IntegrationTests.Auth;

[Collection(ApiCollection.Name)]
public class DevAdminTests
{
    private readonly HttpClient _client;

    public DevAdminTests(JewelryShopApiFactory api)
    {
        _client = api.CreateApiClient();
    }

    [Fact]
    public async Task Permet_de_se_connecter_avec_le_compte_admin_cree_au_demarrage()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = JewelryShopApiFactory.DevAdminEmail,
            Password = JewelryShopApiFactory.DevAdminPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.Equal("Admin", body?.Role);
    }
}
