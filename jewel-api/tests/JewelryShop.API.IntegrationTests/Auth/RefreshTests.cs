using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Features.Auth.DTOs;

namespace JewelryShop.API.IntegrationTests.Auth;

[Collection(ApiCollection.Name)]
public class RefreshTests
{
    private readonly JewelryShopApiFactory _api;
    private readonly HttpClient _client;

    public RefreshTests(JewelryShopApiFactory api)
    {
        _api = api;
        _client = api.CreateApiClient();
    }

    [Fact]
    public async Task Echange_le_cookie_contre_un_nouveau_jwt_et_un_nouveau_refresh_token()
    {
        var refreshToken = await _api.OpenSessionAsync(_client);

        var response = await _client.PostWithRefreshTokenAsync("/api/auth/refresh", refreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var newRefreshToken = await SessionAssertions.AssertSessionOpenedAsync(response);
        Assert.NotEqual(refreshToken, newRefreshToken);
    }

    [Fact]
    public async Task Renvoie_un_jwt_qui_donne_acces_aux_endpoints_authentifies()
    {
        var refreshToken = await _api.OpenSessionAsync(_client);
        var response = await _client.PostWithRefreshTokenAsync("/api/auth/refresh", refreshToken);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/favorites");
        request.Headers.Authorization = new("Bearer", body!.Token);
        var favorites = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, favorites.StatusCode);
    }

    [Fact]
    public async Task Accepte_le_nouveau_refresh_token_pour_la_rotation_suivante()
    {
        var refreshToken = await _api.OpenSessionAsync(_client);
        var newRefreshToken = await _client.RefreshAsync(refreshToken);

        var response = await _client.PostWithRefreshTokenAsync("/api/auth/refresh", newRefreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Refuse_un_refresh_token_deja_tourne_avec_401_et_supprime_le_cookie()
    {
        var refreshToken = await _api.OpenSessionAsync(_client);
        await _client.RefreshAsync(refreshToken);

        var response = await _client.PostWithRefreshTokenAsync("/api/auth/refresh", refreshToken);

        await SessionAssertions.AssertSessionExpiredAsync(response);
        SessionAssertions.AssertRefreshTokenCookieDeleted(response);
    }

    [Fact]
    public async Task Ferme_toutes_les_sessions_quand_un_refresh_token_deja_tourne_est_reutilise()
    {
        var refreshToken = await _api.OpenSessionAsync(_client);
        var newRefreshToken = await _client.RefreshAsync(refreshToken);
        await _client.PostWithRefreshTokenAsync("/api/auth/refresh", refreshToken);

        var response = await _client.PostWithRefreshTokenAsync("/api/auth/refresh", newRefreshToken);

        await SessionAssertions.AssertSessionExpiredAsync(response);
    }

    [Fact]
    public async Task Refuse_une_requete_sans_cookie_avec_401_et_supprime_le_cookie()
    {
        var response = await _client.PostWithRefreshTokenAsync("/api/auth/refresh", refreshToken: null);

        await SessionAssertions.AssertSessionExpiredAsync(response);
        SessionAssertions.AssertRefreshTokenCookieDeleted(response);
    }

    [Fact]
    public async Task Refuse_un_refresh_token_inconnu_avec_401()
    {
        var response = await _client.PostWithRefreshTokenAsync("/api/auth/refresh", "token-inconnu");

        await SessionAssertions.AssertSessionExpiredAsync(response);
    }
}
