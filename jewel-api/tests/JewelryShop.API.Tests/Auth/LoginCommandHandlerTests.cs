using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Common.Enums;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Auth;
using JewelryShop.Application.Features.Auth.Commands;
using JewelryShop.Application.Features.Users;
using JewelryShop.Infrastructure.Services;

namespace JewelryShop.API.Tests.Auth;

public class LoginCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeUserRepository _users = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeRefreshTokenRepository _refreshTokens = new();
    private readonly SecureTokenGenerator _tokenGenerator = new();
    private readonly LoginCommandHandler _handler;
    private readonly User _user;

    public LoginCommandHandlerTests()
    {
        var refreshTokenService = new RefreshTokenService(
            _refreshTokens, _tokenGenerator, new FakeTimeProvider(_now), new ListLogger<RefreshTokenService>());
        _handler = new LoginCommandHandler(_users, _passwordHasher, new FakeJwtService(), refreshTokenService);

        _user = new User
        {
            Id = 7,
            Email = "marie@mycharmly.eu",
            PasswordHash = _passwordHasher.Hash("Secret123!"),
            Role = UserRole.Admin
        };
        _users.Seed(_user);
    }

    [Fact]
    public async Task Renvoie_le_jwt_l_id_et_le_role_de_l_utilisateur()
    {
        var result = await _handler.Handle(new LoginCommand("marie@mycharmly.eu", "Secret123!"), CancellationToken.None);

        Assert.Equal("jwt-7-1", result.Response.Token);
        Assert.Equal(7, result.Response.UserId);
        Assert.Equal("Admin", result.Response.Role);
    }

    [Fact]
    public async Task Emet_un_refresh_token_et_le_sauvegarde()
    {
        var result = await _handler.Handle(new LoginCommand("marie@mycharmly.eu", "Secret123!"), CancellationToken.None);

        var stored = Assert.Single(_refreshTokens.All);
        Assert.Equal(_tokenGenerator.Hash(result.RefreshToken), stored.TokenHash);
        Assert.Equal(_user.Id, stored.UserId);
        Assert.Equal(1, _users.SaveChangesCount);
    }

    [Fact]
    public async Task Normalise_l_email_avant_de_chercher_l_utilisateur()
    {
        var result = await _handler.Handle(new LoginCommand("  Marie@MyCharmly.eu ", "Secret123!"), CancellationToken.None);

        Assert.Equal(7, result.Response.UserId);
    }

    [Theory]
    [InlineData("marie@mycharmly.eu", "MauvaisMotDePasse1!")]
    [InlineData("inconnu@mycharmly.eu", "Secret123!")]
    public async Task Refuse_des_identifiants_incorrects_sans_emettre_de_refresh_token(string email, string password)
    {
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginCommand(email, password), CancellationToken.None));

        Assert.Equal("Email ou mot de passe incorrect.", exception.Message);
        Assert.Empty(_refreshTokens.All);
        Assert.Equal(0, _users.SaveChangesCount);
    }
}
