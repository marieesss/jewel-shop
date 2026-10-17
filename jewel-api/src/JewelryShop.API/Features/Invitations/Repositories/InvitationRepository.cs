using JewelryShop.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Features.Invitations.Repositories;

public class InvitationRepository : Repository<Invitation>, IInvitationRepository
{
    public InvitationRepository(AppDbContext context) : base(context)
    {
    }

    public void Add(Invitation invitation) => DbSet.Add(invitation);

    public async Task<Invitation?> GetByIdWithInviterAsync(long id, CancellationToken ct)
        => await DbSet.Include(i => i.InvitedBy).FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(string email, CancellationToken ct)
        => await DbSet
            .Where(i => i.Email == email && i.Status == InvitationStatus.Pending)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Invitation> Items, int TotalCount)> GetPagedAsync(
        InvitationStatus status, DateTime now, int page, int pageSize, CancellationToken ct)
    {
        var query = DbSet.AsNoTracking().WhereEffectiveStatus(status, now);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByNewest()
            .Include(i => i.InvitedBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Invitation?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
        => DbSet.FirstOrDefaultAsync(i => i.TokenHash == tokenHash, ct);

    public Task<int> ExpirePendingAsync(CancellationToken ct)
        => Context.Database.ExecuteSqlAsync(
            $"UPDATE invitations SET status = 'Expired' WHERE status = 'Pending' AND expires_at <= NOW()", ct);
}
