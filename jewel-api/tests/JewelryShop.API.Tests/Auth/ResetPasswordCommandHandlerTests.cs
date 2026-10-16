using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Common;
using JewelryShop.Features.Auth.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Services;

namespace JewelryShop.API.Tests.Auth;

public class ResetPasswordCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);
    private const string Token = "token-du-lien";

    private readonly User _user = new() { Id = 4, PasswordHash = "hash:Ancien123!" };
    private readonly FakePasswordResetTokenRepository _resetTokens = new();
    private readonly FakeRefreshTokenRepository _refreshTokenRepository = new();
    private readonly SecureTokenGenerator _tokenGenerator = new();
    private readonly FakeTimeProvider _timeProvider = new(_now);
    private readonly RefreshTokenService _refreshTokens;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _refreshTokens = new RefreshTokenService(
            _refreshTokenRepository, _tokenGenerator, _timeProvider, new ListLogger<RefreshTokenService>());
        _handler = new ResetPasswordCommandHandler(
            _resetTokens, _tokenGenerator, new FakePasswordHasher(), _refreshTokens, _timeProvider);
    }

    private PasswordResetToken SeedToken(DateTime createdAt)
    {
        var token = PasswordResetToken.Create(_user, _tokenGenerator.Hash(Token), createdAt);
        _resetTokens.Seed(token);
        return token;
    }

    private Task ResetAsync(string token = Token)
        => _handler.Handle(new ResetPasswordCommand(token, "Nouveau123!"), CancellationToken.None);

    [Fact]
    public async Task Remplace_le_mot_de_passe_et_consomme_le_token()
    {
        var resetToken = SeedToken(_now.UtcDateTime.AddMinutes(-10));

        await ResetAsync();

        Assert.Equal("hash:Nouveau123!", _user.PasswordHash);
        Assert.Equal(_now.UtcDateTime, resetToken.UsedAt);
    }

    [Fact]
    public async Task Revoque_toutes_les_sessions_de_l_utilisateur()
    {
        SeedToken(_now.UtcDateTime.AddMinutes(-10));
        _refreshTokens.Issue(_user);
        _refreshTokens.Issue(_user);

        await ResetAsync();

        Assert.All(_refreshTokenRepository.All, t => Assert.Equal(_now.UtcDateTime, t.RevokedAt));
    }

    [Fact]
    public async Task Enregistre_tout_dans_une_seule_sauvegarde()
    {
        SeedToken(_now.UtcDateTime.AddMinutes(-10));

        await ResetAsync();

        Assert.Equal(1, _resetTokens.SaveChangesCount);
        Assert.Equal(0, _refreshTokenRepository.SaveChangesCount);
    }

    [Fact]
    public async Task Le_token_ne_sert_qu_une_fois()
    {
        SeedToken(_now.UtcDateTime.AddMinutes(-10));
        await ResetAsync();

        await AssertInvalidTokenAsync(() => ResetAsync());
        Assert.Equal(1, _resetTokens.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_un_token_inconnu()
    {
        SeedToken(_now.UtcDateTime.AddMinutes(-10));

        await AssertInvalidTokenAsync(() => ResetAsync("token-inconnu"));
        AssertNothingChanged();
    }

    [Fact]
    public async Task Refuse_un_token_deja_utilise()
    {
        var resetToken = SeedToken(_now.UtcDateTime.AddMinutes(-10));
        resetToken.MarkUsed(_now.UtcDateTime.AddMinutes(-5));

        await AssertInvalidTokenAsync(() => ResetAsync());
        AssertNothingChanged();
    }

    [Fact]
    public async Task Refuse_un_token_cree_il_y_a_exactement_1_heure()
    {
        SeedToken(_now.UtcDateTime - PasswordResetTokenDefaults.Lifetime);

        await AssertInvalidTokenAsync(() => ResetAsync());
        AssertNothingChanged();
    }

    [Fact]
    public async Task Accepte_un_token_cree_juste_avant_l_expiration()
    {
        SeedToken(_now.UtcDateTime - PasswordResetTokenDefaults.Lifetime + TimeSpan.FromSeconds(1));

        await ResetAsync();

        Assert.Equal("hash:Nouveau123!", _user.PasswordHash);
    }

    private static async Task AssertInvalidTokenAsync(Func<Task> reset)
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(reset);
        var messages = Assert.Contains(nameof(ResetPasswordCommand.Token), exception.Errors);
        Assert.Equal([PasswordResetTokenDefaults.InvalidTokenMessage], messages);
    }

    private void AssertNothingChanged()
    {
        Assert.Equal("hash:Ancien123!", _user.PasswordHash);
        Assert.Equal(0, _resetTokens.SaveChangesCount);
    }
}
