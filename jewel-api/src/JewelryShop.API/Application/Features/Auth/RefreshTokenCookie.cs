namespace JewelryShop.Application.Features.Auth;

/// <summary>
/// Cookie qui transporte le refresh token. HttpOnly : inaccessible au JavaScript du front.
/// Limité à <see cref="Path"/> pour n'être envoyé qu'aux endpoints d'authentification.
/// </summary>
public static class RefreshTokenCookie
{
    public const string Name = "refresh_token";
    public const string Path = "/api/auth";

    public static void Append(HttpResponse response, string token)
        => response.Cookies.Append(Name, token, Options(RefreshTokenDefaults.Lifetime));

    public static string? Read(HttpRequest request)
        => request.Cookies.TryGetValue(Name, out var token) && !string.IsNullOrEmpty(token) ? token : null;

    public static void Delete(HttpResponse response) => response.Cookies.Delete(Name, Options(maxAge: null));

    // localhost et 127.0.0.1 sont des contextes sécurisés : le cookie Secure fonctionne aussi en local en HTTP.
    private static CookieOptions Options(TimeSpan? maxAge) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        MaxAge = maxAge
    };
}
