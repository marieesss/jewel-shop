using JewelryShop.Features.Invitations.Common;
using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Invitations.Repositories;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>
/// L'expiration en masse est faite en SQL : ici, ExpirePendingAsync compte seulement ses appels.
/// La liste reprend les filtres et le tri de <see cref="InvitationQueries"/>, comme le repository EF.
/// </summary>
public class FakeInvitationRepository : InMemoryRepository<Invitation>, IInvitationRepository
{
    public int ExpirePendingCount { get; private set; }

    public void Add(Invitation invitation) => Entities.Add(invitation);

    public Task<Invitation?> GetByIdWithInviterAsync(long id, CancellationToken ct) => GetByIdAsync(id, ct);

    public Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(string email, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Invitation>>(
            Entities.Where(i => i.Email == email && i.Status == InvitationStatus.Pending).ToList());

    public Task<(IReadOnlyList<Invitation> Items, int TotalCount)> GetPagedAsync(
        InvitationStatus status, DateTime now, int page, int pageSize, CancellationToken ct)
    {
        var query = Entities.AsQueryable().WhereEffectiveStatus(status, now);
        var items = query.OrderByNewest().Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<Invitation>, int)>((items, query.Count()));
    }

    public Task<Invitation?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
        => Task.FromResult(Entities.FirstOrDefault(i => i.TokenHash == tokenHash));

    public Task<int> ExpirePendingAsync(CancellationToken ct)
    {
        ExpirePendingCount++;
        return Task.FromResult(0);
    }
}
