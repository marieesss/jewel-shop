using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Features.Auth.DTOs;

namespace JewelryShop.API.IntegrationTests.Auth;

[Collection(ApiCollection.Name)]
public class LoginTests
{
    private readonly JewelryShopApiFactory _api;
    private readonly HttpClient _client;

    public LoginTests(JewelryShopApiFactory api)
    {
        _api = api;
        _client = api.CreateApiClient();
    }

    [Fact]
    public async Task Renvoie_un_jwt_et_pose_le_cookie_du_refresh_token()
    {
        var user = await _api.CreateUserAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { user.Email, Password = TestAccounts.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await SessionAssertions.AssertSessionOpenedAsync(response);
    }

    [Fact]
    public async Task Renvoie_l_identifiant_et_le_role_du_compte()
    {
        var user = await _api.CreateUserAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { user.Email, Password = TestAccounts.Password });

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.Equal(user.Id, body?.UserId);
        Assert.Equal("User", body?.Role);
    }

    [Fact]
    public async Task Refuse_un_mot_de_passe_incorrect_avec_401_sans_poser_de_cookie()
    {
        var user = await _api.CreateUserAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { user.Email, Password = "Mauvais1!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal("Email ou mot de passe incorrect.", error?.Title);
        Assert.Null(RefreshTokenCookies.Find(response));
    }

    [Fact]
    public async Task Refuse_un_email_vide_avec_400_et_le_detail_de_l_erreur()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = "", Password = TestAccounts.Password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(400, error?.Status);
        Assert.NotNull(error?.Errors);
        Assert.Contains("Email", error.Errors.Keys);
    }
}
