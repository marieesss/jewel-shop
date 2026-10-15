using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Auth;
using JewelryShop.Application.Features.Auth.Commands;
using JewelryShop.Application.Features.Users;
using JewelryShop.Infrastructure.Services;

namespace JewelryShop.API.Tests.Auth;

public class LogoutCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly User _user = new() { Id = 4 };
    private readonly FakeRefreshTokenRepository _repository = new();
    private readonly RefreshTokenService _refreshTokens;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _refreshTokens = new RefreshTokenService(
            _repository, new SecureTokenGenerator(), new FakeTimeProvider(_now), new ListLogger<RefreshTokenService>());
        _handler = new LogoutCommandHandler(_refreshTokens);
    }

    private Task LogoutAsync(string? token) => _handler.Handle(new LogoutCommand(token), CancellationToken.None);

    [Fact]
    public async Task Revoque_le_token_qui_ne_permet_plus_de_rafraichir_la_session()
    {
        var token = _refreshTokens.Issue(_user);

        await LogoutAsync(token);

        Assert.Equal(_now.UtcDateTime, Assert.Single(_repository.All).RevokedAt);
        await Assert.ThrowsAsync<UnauthorizedException>(() => _refreshTokens.RotateAsync(token, CancellationToken.None));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("token-inconnu")]
    public async Task Accepte_un_token_absent_ou_inconnu_sans_rien_sauvegarder(string? token)
    {
        await LogoutAsync(token);

        Assert.Equal(0, _repository.SaveChangesCount);
    }
}
