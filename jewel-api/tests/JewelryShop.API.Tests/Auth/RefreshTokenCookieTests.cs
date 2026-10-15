using JewelryShop.Application.Features.Auth;
using Microsoft.AspNetCore.Http;

namespace JewelryShop.API.Tests.Auth;

public class RefreshTokenCookieTests
{
    private readonly DefaultHttpContext _context = new();

    private string SetCookieHeader => Assert.Single(_context.Response.Headers.SetCookie)!;

    private string[] SetCookieAttributes =>
        SetCookieHeader.Split(';', StringSplitOptions.TrimEntries).Select(a => a.ToLowerInvariant()).ToArray();

    [Fact]
    public void Append_pose_un_cookie_httponly_secure_strict_limite_a_api_auth_pour_30_jours()
    {
        RefreshTokenCookie.Append(_context.Response, "abc");

        Assert.StartsWith("refresh_token=abc;", SetCookieHeader);
        Assert.Contains("path=/api/auth", SetCookieAttributes);
        Assert.Contains("httponly", SetCookieAttributes);
        Assert.Contains("secure", SetCookieAttributes);
        Assert.Contains("samesite=strict", SetCookieAttributes);
        Assert.Contains("max-age=2592000", SetCookieAttributes);
    }

    [Fact]
    public void Delete_expire_le_cookie_sur_le_meme_chemin()
    {
        RefreshTokenCookie.Delete(_context.Response);

        Assert.StartsWith("refresh_token=;", SetCookieHeader);
        Assert.Contains("path=/api/auth", SetCookieAttributes);
        Assert.Contains("expires=thu, 01 jan 1970 00:00:00 gmt", SetCookieAttributes);
    }

    [Fact]
    public void Read_renvoie_la_valeur_du_cookie()
    {
        _context.Request.Headers.Cookie = "autre=1; refresh_token=abc";

        Assert.Equal("abc", RefreshTokenCookie.Read(_context.Request));
    }

    [Theory]
    [InlineData("")]
    [InlineData("autre=1")]
    [InlineData("refresh_token=")]
    public void Read_renvoie_null_sans_cookie_ou_avec_un_cookie_vide(string cookieHeader)
    {
        _context.Request.Headers.Cookie = cookieHeader;

        Assert.Null(RefreshTokenCookie.Read(_context.Request));
    }
}
