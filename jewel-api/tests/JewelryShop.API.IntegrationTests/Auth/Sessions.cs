using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;

namespace JewelryShop.API.IntegrationTests.Auth;

public static class Sessions
{
    /// <summary>Crée un compte, s'y connecte et renvoie le refresh token posé en cookie.</summary>
    public static async Task<string> OpenSessionAsync(this JewelryShopApiFactory api, HttpClient client)
    {
        var user = await api.CreateUserAsync();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { user.Email, Password = TestAccounts.Password });
        response.EnsureSuccessStatusCode();
        return RefreshTokenCookies.Get(response).Value.Value!;
    }

    public static async Task<string> RefreshAsync(this HttpClient client, string refreshToken)
    {
        var response = await client.PostWithRefreshTokenAsync("/api/auth/refresh", refreshToken);
        response.EnsureSuccessStatusCode();
        return RefreshTokenCookies.Get(response).Value.Value!;
    }
}
