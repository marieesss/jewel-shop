using JewelryShop.Application.Features.Auth;
using JewelryShop.Application.Features.Users;

namespace JewelryShop.API.Tests.Auth;

public class RefreshTokenTests
{
    private static readonly DateTime _now = new(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc);

    private static RefreshToken CreateToken()
        => RefreshToken.Create(new User { Id = 1 }, new string('a', 64), _now);

    [Fact]
    public void Un_nouveau_token_expire_dans_30_jours()
    {
        var token = CreateToken();

        Assert.Equal(1, token.UserId);
        Assert.Equal(_now.AddDays(30), token.ExpiresAt);
    }

    [Fact]
    public void Un_nouveau_token_est_actif()
    {
        Assert.True(CreateToken().IsActive(_now));
    }

    [Fact]
    public void Un_token_tourne_n_est_plus_actif()
    {
        var token = CreateToken();

        token.MarkRotated(_now);

        Assert.False(token.IsActive(_now));
    }

    [Fact]
    public void Un_token_revoque_n_est_plus_actif()
    {
        var token = CreateToken();

        token.Revoke(_now);

        Assert.False(token.IsActive(_now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Un_token_n_est_plus_actif_a_partir_de_son_expiration(int secondsAfterExpiration)
    {
        var token = CreateToken();

        Assert.False(token.IsActive(token.ExpiresAt.AddSeconds(secondsAfterExpiration)));
    }

    [Fact]
    public void Un_token_est_encore_actif_juste_avant_son_expiration()
    {
        var token = CreateToken();

        Assert.True(token.IsActive(token.ExpiresAt.AddSeconds(-1)));
    }
}
