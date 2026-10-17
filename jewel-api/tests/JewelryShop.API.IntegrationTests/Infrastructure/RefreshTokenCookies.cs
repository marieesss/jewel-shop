using JewelryShop.Features.Auth.Common;
using Microsoft.Net.Http.Headers;

namespace JewelryShop.API.IntegrationTests.Infrastructure;

/// <summary>Lit le cookie du refresh token dans une réponse et l'ajoute à une requête, comme le ferait le navigateur.</summary>
public static class RefreshTokenCookies
{
    public static SetCookieHeaderValue? Find(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues(HeaderNames.SetCookie, out var headers))
            return null;

        return SetCookieHeaderValue.ParseList(headers.ToList())
            .SingleOrDefault(cookie => cookie.Name == RefreshTokenCookie.Name);
    }

    public static SetCookieHeaderValue Get(HttpResponseMessage response)
        => Find(response) ?? throw new InvalidOperationException("La réponse ne pose pas le cookie du refresh token.");

    public static Task<HttpResponseMessage> PostWithRefreshTokenAsync(this HttpClient client, string path, string? refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        if (refreshToken is not null)
            request.Headers.Add(HeaderNames.Cookie, $"{RefreshTokenCookie.Name}={refreshToken}");
        return client.SendAsync(request);
    }
}
