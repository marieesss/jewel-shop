using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Invitations.Repositories;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>
/// L'expiration en masse est faite en SQL : ici, ExpirePendingAsync compte seulement ses appels.
/// </summary>
public class FakeInvitationRepository : InMemoryRepository<Invitation>, IInvitationRepository
{
    public int ExpirePendingCount { get; private set; }

    public void Add(Invitation invitation) => Entities.Add(invitation);

    public Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(string email, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Invitation>>(
            Entities.Where(i => i.Email == email && i.Status == InvitationStatus.Pending).ToList());

    public Task<int> ExpirePendingAsync(CancellationToken ct)
    {
        ExpirePendingCount++;
        return Task.FromResult(0);
    }
}
