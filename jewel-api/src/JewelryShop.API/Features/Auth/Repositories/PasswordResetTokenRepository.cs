using JewelryShop.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Features.Auth.Repositories;

public class PasswordResetTokenRepository : Repository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(AppDbContext context) : base(context)
    {
    }

    public void Add(PasswordResetToken token) => DbSet.Add(token);

    public Task<PasswordResetToken?> GetByHashWithUserAsync(string tokenHash, CancellationToken ct)
        => DbSet
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<PasswordResetToken>> GetUnusedByUserIdAsync(long userId, CancellationToken ct)
        => await DbSet
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ToListAsync(ct);

    public Task<int> DeleteExpiredOrUsedAsync(CancellationToken ct)
        => Context.Database.ExecuteSqlAsync(
            $"DELETE FROM password_reset_tokens WHERE expires_at < NOW() OR used_at IS NOT NULL", ct);
}
