using JewelryShop.Application.Features.Auth;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>
/// La purge est faite en SQL : ici, DeleteExpiredAsync compte seulement ses appels.
/// </summary>
public class FakeRefreshTokenRepository : InMemoryRepository<RefreshToken>, IRefreshTokenRepository
{
    public int PurgeCount { get; private set; }

    public void Add(RefreshToken token) => Entities.Add(token);

    public Task<RefreshToken?> GetByHashWithUserAsync(string tokenHash, CancellationToken ct)
        => Task.FromResult(Entities.FirstOrDefault(t => t.TokenHash == tokenHash));

    public Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(long userId, DateTime now, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<RefreshToken>>(
            Entities.Where(t => t.UserId == userId && t.IsActive(now)).ToList());

    public Task<int> DeleteExpiredAsync(CancellationToken ct)
    {
        PurgeCount++;
        return Task.FromResult(0);
    }
}
