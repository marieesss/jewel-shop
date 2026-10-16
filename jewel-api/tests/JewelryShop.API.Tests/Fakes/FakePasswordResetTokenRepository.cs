using JewelryShop.Features.Auth.Entities;
using JewelryShop.Features.Auth.Repositories;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>
/// La purge est faite en SQL : ici, DeleteExpiredOrUsedAsync compte seulement ses appels.
/// </summary>
public class FakePasswordResetTokenRepository : InMemoryRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public int PurgeCount { get; private set; }

    public void Add(PasswordResetToken token) => Entities.Add(token);

    public Task<PasswordResetToken?> GetByHashWithUserAsync(string tokenHash, CancellationToken ct)
        => Task.FromResult(Entities.FirstOrDefault(t => t.TokenHash == tokenHash));

    public Task<IReadOnlyList<PasswordResetToken>> GetUnusedByUserIdAsync(long userId, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<PasswordResetToken>>(
            Entities.Where(t => t.UserId == userId && !t.IsUsed).ToList());

    public Task<int> DeleteExpiredOrUsedAsync(CancellationToken ct)
    {
        PurgeCount++;
        return Task.FromResult(0);
    }
}
