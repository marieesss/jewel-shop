using JewelryShop.Features.Auth.Entities;
using JewelryShop.Features.Users.Entities;

namespace JewelryShop.API.Tests.Auth;

public class PasswordResetTokenTests
{
    private static readonly DateTime _now = new(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc);

    private static PasswordResetToken CreateToken()
        => PasswordResetToken.Create(new User { Id = 1 }, new string('a', 64), _now);

    [Fact]
    public void Un_nouveau_token_expire_dans_1_heure()
    {
        var token = CreateToken();

        Assert.Equal(1, token.UserId);
        Assert.Equal(_now.AddHours(1), token.ExpiresAt);
    }

    [Fact]
    public void Un_nouveau_token_est_utilisable()
    {
        Assert.True(CreateToken().IsUsable(_now));
    }

    [Fact]
    public void Un_token_reste_utilisable_juste_avant_son_expiration()
    {
        Assert.True(CreateToken().IsUsable(_now.AddHours(1).AddTicks(-1)));
    }

    [Fact]
    public void Un_token_n_est_plus_utilisable_exactement_a_son_expiration()
    {
        Assert.False(CreateToken().IsUsable(_now.AddHours(1)));
    }

    [Fact]
    public void Un_token_utilise_n_est_plus_utilisable()
    {
        var token = CreateToken();

        token.MarkUsed(_now);

        Assert.True(token.IsUsed);
        Assert.False(token.IsUsable(_now));
    }
}
