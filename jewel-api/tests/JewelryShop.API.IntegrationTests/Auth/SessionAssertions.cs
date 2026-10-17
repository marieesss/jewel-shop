using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Features.Auth.Common;
using JewelryShop.Features.Auth.DTOs;
using JewelryShop.Infrastructure.Services;
using Microsoft.Net.Http.Headers;

namespace JewelryShop.API.IntegrationTests.Auth;

public static class SessionAssertions
{
    /// <summary>Vérifie le JWT du corps et le cookie du refresh token, puis renvoie la valeur du cookie.</summary>
    public static async Task<string> AssertSessionOpenedAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrEmpty(body.Token));

        var cookie = RefreshTokenCookies.Get(response);
        AssertRefreshTokenCookieAttributes(cookie);
        Assert.Equal(RefreshTokenDefaults.Lifetime, cookie.MaxAge);
        Assert.False(string.IsNullOrEmpty(cookie.Value.Value));
        return cookie.Value.Value!;
    }

    public static void AssertRefreshTokenCookieDeleted(HttpResponseMessage response)
    {
        var cookie = RefreshTokenCookies.Get(response);
        AssertRefreshTokenCookieAttributes(cookie);
        Assert.True(string.IsNullOrEmpty(cookie.Value.Value));
        Assert.NotNull(cookie.Expires);
        Assert.True(cookie.Expires < DateTimeOffset.UtcNow);
    }

    public static async Task AssertSessionExpiredAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(RefreshTokenService.SessionExpiredMessage, error?.Title);
    }

    private static void AssertRefreshTokenCookieAttributes(SetCookieHeaderValue cookie)
    {
        Assert.True(cookie.HttpOnly);
        Assert.True(cookie.Secure);
        Assert.Equal(SameSiteMode.Strict, cookie.SameSite);
        Assert.Equal(RefreshTokenCookie.Path, cookie.Path.Value);
    }
}
