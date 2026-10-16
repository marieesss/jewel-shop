using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;

namespace JewelryShop.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    public const string SessionExpiredMessage = "Votre session a expiré. Veuillez vous reconnecter.";

    private readonly IRefreshTokenRepository _repository;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IRefreshTokenRepository repository,
        ISecureTokenGenerator tokenGenerator,
        TimeProvider timeProvider,
        ILogger<RefreshTokenService> logger)
    {
        _repository = repository;
        _tokenGenerator = tokenGenerator;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public string Issue(User user)
    {
        var token = _tokenGenerator.Generate();
        _repository.Add(RefreshToken.Create(user, _tokenGenerator.Hash(token), UtcNow));
        return token;
    }

    public async Task<(User User, string NewToken)> RotateAsync(string token, CancellationToken ct)
    {
        var now = UtcNow;
        var current = await FindAsync(token, ct);

        if (current is null || current.IsRevoked || current.IsExpired(now))
            throw new UnauthorizedException(SessionExpiredMessage);

        if (current.IsRotated)
        {
            await RevokeAllForUserAsync(current.UserId, ct);
            await _repository.SaveChangesAsync(ct);
            _logger.LogWarning(
                "Réutilisation d'un refresh token déjà tourné : sessions de l'utilisateur {UserId} révoquées.",
                current.UserId);
            throw new UnauthorizedException(SessionExpiredMessage);
        }

        current.MarkRotated(now);
        var newToken = Issue(current.User);
        await _repository.SaveChangesAsync(ct);
        return (current.User, newToken);
    }

    public async Task RevokeAsync(string token, CancellationToken ct)
    {
        var now = UtcNow;
        var current = await FindAsync(token, ct);
        if (current is null || !current.IsActive(now))
            return;

        current.Revoke(now);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(long userId, CancellationToken ct)
    {
        var now = UtcNow;
        foreach (var activeToken in await _repository.GetActiveByUserIdAsync(userId, now, ct))
            activeToken.Revoke(now);
    }

    private Task<RefreshToken?> FindAsync(string token, CancellationToken ct)
        => _repository.GetByHashWithUserAsync(_tokenGenerator.Hash(token), ct);

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
