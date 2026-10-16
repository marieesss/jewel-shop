using JewelryShop.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Features.Auth.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    public void Add(RefreshToken token) => DbSet.Add(token);

    public Task<RefreshToken?> GetByHashWithUserAsync(string tokenHash, CancellationToken ct)
        => DbSet
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        long userId, DateTime now, CancellationToken ct)
        => await DbSet
            .Where(t => t.UserId == userId
                && t.RotatedAt == null
                && t.RevokedAt == null
                && t.ExpiresAt > now)
            .ToListAsync(ct);

    public Task<int> DeleteExpiredAsync(CancellationToken ct)
        => Context.Database.ExecuteSqlAsync($"DELETE FROM refresh_tokens WHERE expires_at < NOW()", ct);
}
