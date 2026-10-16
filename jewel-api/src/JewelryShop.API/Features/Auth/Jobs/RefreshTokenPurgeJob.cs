using JewelryShop.Common.Interfaces;

namespace JewelryShop.Features.Auth.Jobs;

/// <summary>
/// Supprime les refresh tokens expirés. Les tokens tournés ou révoqués sont gardés jusqu'à leur
/// expiration pour détecter une réutilisation.
/// </summary>
public class RefreshTokenPurgeJob : IPeriodicJob
{
    private readonly IRefreshTokenRepository _repository;

    public RefreshTokenPurgeJob(IRefreshTokenRepository repository)
    {
        _repository = repository;
    }

    public TimeSpan Interval => TimeSpan.FromHours(1);

    public Task RunAsync(CancellationToken ct) => _repository.DeleteExpiredAsync(ct);
}
