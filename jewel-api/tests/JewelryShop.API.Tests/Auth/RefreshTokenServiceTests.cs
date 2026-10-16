using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Auth.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace JewelryShop.API.Tests.Auth;

public class RefreshTokenServiceTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);
    private const string SessionExpiredMessage = "Votre session a expiré. Veuillez vous reconnecter.";

    private readonly User _user = new() { Id = 1 };
    private readonly User _otherUser = new() { Id = 2 };
    private readonly FakeRefreshTokenRepository _repository = new();
    private readonly SecureTokenGenerator _tokenGenerator = new();
    private readonly FakeTimeProvider _timeProvider = new(_now);
    private readonly ListLogger<RefreshTokenService> _logger = new();
    private readonly RefreshTokenService _service;

    public RefreshTokenServiceTests()
    {
        _service = new RefreshTokenService(_repository, _tokenGenerator, _timeProvider, _logger);
    }

    private RefreshToken StoredToken(string token) => _repository.All.Single(t => t.TokenHash == _tokenGenerator.Hash(token));

    private void AdvanceTime(TimeSpan duration) => _timeProvider.Now += duration;

    [Fact]
    public async Task Issue_enregistre_le_hash_du_token_avec_une_expiration_a_30_jours()
    {
        var token = _service.Issue(_user);
        await _repository.SaveChangesAsync();

        var stored = Assert.Single(_repository.All);
        Assert.Equal(64, stored.TokenHash.Length);
        Assert.NotEqual(token, stored.TokenHash);
        Assert.Equal(_user.Id, stored.UserId);
        Assert.Equal(_now.UtcDateTime.AddDays(30), stored.ExpiresAt);
    }

    [Fact]
    public void Issue_ne_sauvegarde_pas_le_token_lui_meme()
    {
        _service.Issue(_user);

        Assert.Equal(0, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RotateAsync_marque_l_ancien_token_comme_tourne()
    {
        var token = _service.Issue(_user);

        await _service.RotateAsync(token, CancellationToken.None);

        Assert.Equal(_now.UtcDateTime, StoredToken(token).RotatedAt);
    }

    [Fact]
    public async Task RotateAsync_renvoie_l_utilisateur_et_un_nouveau_token_valable_30_jours_de_plus()
    {
        var token = _service.Issue(_user);
        AdvanceTime(TimeSpan.FromDays(10));

        var (user, newToken) = await _service.RotateAsync(token, CancellationToken.None);

        Assert.Same(_user, user);
        Assert.NotEqual(token, newToken);
        var stored = StoredToken(newToken);
        Assert.True(stored.IsActive(_timeProvider.Now.UtcDateTime));
        Assert.Equal(_timeProvider.Now.UtcDateTime.AddDays(30), stored.ExpiresAt);
        Assert.Equal(1, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RotateAsync_d_un_token_deja_tourne_revoque_toutes_les_sessions_de_l_utilisateur()
    {
        var reusedToken = _service.Issue(_user);
        var (_, currentToken) = await _service.RotateAsync(reusedToken, CancellationToken.None);
        var otherDeviceToken = _service.Issue(_user);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.RotateAsync(reusedToken, CancellationToken.None));

        Assert.Equal(SessionExpiredMessage, exception.Message);
        Assert.NotNull(StoredToken(currentToken).RevokedAt);
        Assert.NotNull(StoredToken(otherDeviceToken).RevokedAt);
        Assert.Equal(2, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RotateAsync_d_un_token_deja_tourne_journalise_un_warning_avec_l_id_de_l_utilisateur()
    {
        var reusedToken = _service.Issue(_user);
        await _service.RotateAsync(reusedToken, CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RotateAsync(reusedToken, CancellationToken.None));

        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains(_user.Id.ToString(), entry.Message);
    }

    [Fact]
    public async Task RotateAsync_d_un_token_deja_tourne_ne_revoque_pas_les_tokens_d_un_autre_utilisateur()
    {
        var reusedToken = _service.Issue(_user);
        await _service.RotateAsync(reusedToken, CancellationToken.None);
        var otherUserToken = _service.Issue(_otherUser);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RotateAsync(reusedToken, CancellationToken.None));

        Assert.True(StoredToken(otherUserToken).IsActive(_now.UtcDateTime));
    }

    [Fact]
    public async Task RotateAsync_d_un_token_inconnu_leve_une_UnauthorizedException()
    {
        var otherToken = _service.Issue(_user);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.RotateAsync(_tokenGenerator.Generate(), CancellationToken.None));

        Assert.Equal(SessionExpiredMessage, exception.Message);
        Assert.True(StoredToken(otherToken).IsActive(_now.UtcDateTime));
        Assert.Equal(0, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RotateAsync_d_un_token_expire_leve_une_UnauthorizedException_sans_rien_revoquer()
    {
        var expiredToken = _service.Issue(_user);
        AdvanceTime(TimeSpan.FromDays(30));
        var activeToken = _service.Issue(_user);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.RotateAsync(expiredToken, CancellationToken.None));

        Assert.Equal(SessionExpiredMessage, exception.Message);
        Assert.Null(StoredToken(expiredToken).RotatedAt);
        Assert.True(StoredToken(activeToken).IsActive(_timeProvider.Now.UtcDateTime));
        Assert.Equal(0, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RotateAsync_d_un_token_revoque_leve_une_UnauthorizedException_sans_rien_revoquer()
    {
        var revokedToken = _service.Issue(_user);
        var activeToken = _service.Issue(_user);
        await _service.RevokeAsync(revokedToken, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _service.RotateAsync(revokedToken, CancellationToken.None));

        Assert.Equal(SessionExpiredMessage, exception.Message);
        Assert.True(StoredToken(activeToken).IsActive(_now.UtcDateTime));
        Assert.Equal(1, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RevokeAsync_revoque_un_token_actif_et_sauvegarde()
    {
        var token = _service.Issue(_user);

        await _service.RevokeAsync(token, CancellationToken.None);

        Assert.Equal(_now.UtcDateTime, StoredToken(token).RevokedAt);
        Assert.Equal(1, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RevokeAsync_ne_fait_rien_pour_un_token_inconnu()
    {
        await _service.RevokeAsync(_tokenGenerator.Generate(), CancellationToken.None);

        Assert.Equal(0, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RevokeAsync_ne_fait_rien_pour_un_token_deja_tourne()
    {
        var token = _service.Issue(_user);
        await _service.RotateAsync(token, CancellationToken.None);

        await _service.RevokeAsync(token, CancellationToken.None);

        Assert.Null(StoredToken(token).RevokedAt);
        Assert.Equal(1, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RevokeAllForUserAsync_revoque_les_tokens_actifs_de_l_utilisateur_sans_sauvegarder()
    {
        var firstToken = _service.Issue(_user);
        var secondToken = _service.Issue(_user);

        await _service.RevokeAllForUserAsync(_user.Id, CancellationToken.None);

        Assert.Equal(_now.UtcDateTime, StoredToken(firstToken).RevokedAt);
        Assert.Equal(_now.UtcDateTime, StoredToken(secondToken).RevokedAt);
        Assert.Equal(0, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task RevokeAllForUserAsync_laisse_actifs_les_tokens_d_un_autre_utilisateur()
    {
        _service.Issue(_user);
        var otherUserToken = _service.Issue(_otherUser);

        await _service.RevokeAllForUserAsync(_user.Id, CancellationToken.None);

        Assert.True(StoredToken(otherUserToken).IsActive(_now.UtcDateTime));
    }

    [Fact]
    public async Task Le_token_en_clair_n_apparait_dans_aucun_log()
    {
        var reusedToken = _service.Issue(_user);
        var (_, currentToken) = await _service.RotateAsync(reusedToken, CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RotateAsync(reusedToken, CancellationToken.None));

        Assert.All(_logger.Entries, entry =>
        {
            Assert.DoesNotContain(reusedToken, entry.Message);
            Assert.DoesNotContain(currentToken, entry.Message);
        });
    }
}
