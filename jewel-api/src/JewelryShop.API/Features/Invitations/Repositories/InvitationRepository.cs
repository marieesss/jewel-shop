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

    public async Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(string email, CancellationToken ct)
        => await DbSet
            .Where(i => i.Email == email && i.Status == InvitationStatus.Pending)
            .ToListAsync(ct);

    public Task<int> ExpirePendingAsync(CancellationToken ct)
        => Context.Database.ExecuteSqlAsync(
            $"UPDATE invitations SET status = 'Expired' WHERE status = 'Pending' AND expires_at <= NOW()", ct);
}
