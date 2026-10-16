using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Invitations.Jobs;

namespace JewelryShop.API.Tests.Invitations;

public class InvitationExpirationJobTests
{
    [Fact]
    public async Task Expire_les_invitations_en_attente_depassees()
    {
        var repository = new FakeInvitationRepository();

        await new InvitationExpirationJob(repository).Invoke();

        Assert.Equal(1, repository.ExpirePendingCount);
    }
}
