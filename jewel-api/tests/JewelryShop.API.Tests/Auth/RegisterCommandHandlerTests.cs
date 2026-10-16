using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Services;

namespace JewelryShop.API.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeUserRepository _users = new();
    private readonly FakeRefreshTokenRepository _refreshTokens = new();
    private readonly SecureTokenGenerator _tokenGenerator = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        var refreshTokenService = new RefreshTokenService(
            _refreshTokens, _tokenGenerator, new FakeTimeProvider(_now), new ListLogger<RefreshTokenService>());
        _handler = new RegisterCommandHandler(_users, new FakePasswordHasher(), new FakeJwtService(), refreshTokenService);
    }

    private static RegisterCommand ValidCommand() =>
        new("Marie", "Dupont", "Marie@MyCharmly.eu", "Secret123!", new DateOnly(1990, 5, 12));

    [Fact]
    public async Task Cree_l_utilisateur_avec_le_role_user()
    {
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        var user = Assert.Single(_users.All);
        Assert.Equal("marie@mycharmly.eu", user.Email);
        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal("User", result.Response.Role);
    }

    [Fact]
    public async Task Emet_le_refresh_token_dans_la_meme_sauvegarde_que_l_utilisateur()
    {
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        var stored = Assert.Single(_refreshTokens.All);
        Assert.Same(_users.All.Single(), stored.User);
        Assert.Equal(_tokenGenerator.Hash(result.RefreshToken), stored.TokenHash);
        Assert.Equal(1, _users.SaveChangesCount);
        Assert.Equal(0, _refreshTokens.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_un_email_deja_utilise_sans_emettre_de_refresh_token()
    {
        _users.Seed(new User { Id = 1, Email = "marie@mycharmly.eu" });

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(ValidCommand(), CancellationToken.None));

        Assert.Equal("Un compte existe déjà avec cet email.", exception.Message);
        Assert.Empty(_refreshTokens.All);
        Assert.Equal(0, _users.SaveChangesCount);
    }
}
