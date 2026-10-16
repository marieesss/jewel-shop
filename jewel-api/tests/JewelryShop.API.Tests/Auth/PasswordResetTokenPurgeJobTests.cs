using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Auth.Jobs;

namespace JewelryShop.API.Tests.Auth;

public class PasswordResetTokenPurgeJobTests
{
    [Fact]
    public async Task Supprime_les_tokens_de_reinitialisation_expires_ou_utilises()
    {
        var repository = new FakePasswordResetTokenRepository();

        await new PasswordResetTokenPurgeJob(repository).Invoke();

        Assert.Equal(1, repository.PurgeCount);
    }
}
