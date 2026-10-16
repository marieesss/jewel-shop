using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Auth;
using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Services;

namespace JewelryShop.API.Tests.Auth;

public class RefreshSessionCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly User _user = new() { Id = 3, Role = UserRole.User };
    private readonly FakeRefreshTokenRepository _repository = new();
    private readonly RefreshTokenService _refreshTokens;
    private readonly RefreshSessionCommandHandler _handler;

    public RefreshSessionCommandHandlerTests()
    {
        _refreshTokens = new RefreshTokenService(
            _repository, new SecureTokenGenerator(), new FakeTimeProvider(_now), new ListLogger<RefreshTokenService>());
        _handler = new RefreshSessionCommandHandler(_refreshTokens, new FakeJwtService());
    }

    private Task<AuthResult> RefreshAsync(string? token)
        => _handler.Handle(new RefreshSessionCommand(token), CancellationToken.None);

    [Fact]
    public async Task Renvoie_un_nouveau_jwt_et_un_nouveau_refresh_token()
    {
        var token = _refreshTokens.Issue(_user);

        var result = await RefreshAsync(token);

        Assert.Equal("jwt-3-1", result.Response.Token);
        Assert.Equal(3, result.Response.UserId);
        Assert.Equal("User", result.Response.Role);
        Assert.NotEqual(token, result.RefreshToken);
    }

    [Fact]
    public async Task Le_nouveau_refresh_token_permet_un_nouveau_rafraichissement()
    {
        var first = await RefreshAsync(_refreshTokens.Issue(_user));

        var second = await RefreshAsync(first.RefreshToken);

        Assert.Equal("jwt-3-2", second.Response.Token);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("token-inconnu")]
    public async Task Refuse_un_token_absent_ou_inconnu(string? token)
    {
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => RefreshAsync(token));

        Assert.Equal("Votre session a expiré. Veuillez vous reconnecter.", exception.Message);
    }

    [Fact]
    public async Task Refuse_un_token_deja_tourne_et_invalide_le_nouveau()
    {
        var oldToken = _refreshTokens.Issue(_user);
        var rotated = await RefreshAsync(oldToken);

        await Assert.ThrowsAsync<UnauthorizedException>(() => RefreshAsync(oldToken));

        await Assert.ThrowsAsync<UnauthorizedException>(() => RefreshAsync(rotated.RefreshToken));
    }
}
