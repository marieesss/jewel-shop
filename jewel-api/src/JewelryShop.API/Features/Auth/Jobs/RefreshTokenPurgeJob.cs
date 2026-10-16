using Coravel.Invocable;

namespace JewelryShop.Features.Auth.Jobs;

/// <summary>
/// Supprime les refresh tokens expirés. Les tokens tournés ou révoqués sont gardés jusqu'à leur
/// expiration pour détecter une réutilisation.
/// </summary>
public class RefreshTokenPurgeJob : IInvocable, ICancellableInvocable
{
    private readonly IRefreshTokenRepository _repository;

    public RefreshTokenPurgeJob(IRefreshTokenRepository repository)
    {
        _repository = repository;
    }

    public CancellationToken CancellationToken { get; set; }

    public Task Invoke() => _repository.DeleteExpiredAsync(CancellationToken);
}
