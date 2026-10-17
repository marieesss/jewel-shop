using System.Net;
using JewelryShop.API.IntegrationTests.Infrastructure;

namespace JewelryShop.API.IntegrationTests.Auth;

[Collection(ApiCollection.Name)]
public class LogoutTests
{
    private readonly JewelryShopApiFactory _api;
    private readonly HttpClient _client;

    public LogoutTests(JewelryShopApiFactory api)
    {
        _api = api;
        _client = api.CreateApiClient();
    }

    [Fact]
    public async Task Repond_204_et_supprime_le_cookie()
    {
        var refreshToken = await _api.OpenSessionAsync(_client);

        var response = await _client.PostWithRefreshTokenAsync("/api/auth/logout", refreshToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        SessionAssertions.AssertRefreshTokenCookieDeleted(response);
    }

    [Fact]
    public async Task Revoque_le_refresh_token_qui_ne_permet_plus_de_rafraichir_la_session()
    {
        var refreshToken = await _api.OpenSessionAsync(_client);
        await _client.PostWithRefreshTokenAsync("/api/auth/logout", refreshToken);

        var response = await _client.PostWithRefreshTokenAsync("/api/auth/refresh", refreshToken);

        await SessionAssertions.AssertSessionExpiredAsync(response);
    }

    [Fact]
    public async Task Repond_204_et_supprime_le_cookie_sans_cookie()
    {
        var response = await _client.PostWithRefreshTokenAsync("/api/auth/logout", refreshToken: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        SessionAssertions.AssertRefreshTokenCookieDeleted(response);
    }
}
