using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Auth.Jobs;

namespace JewelryShop.API.Tests.Auth;

public class RefreshTokenPurgeJobTests
{
    [Fact]
    public async Task Supprime_les_refresh_tokens_expires()
    {
        var repository = new FakeRefreshTokenRepository();

        await new RefreshTokenPurgeJob(repository).Invoke();

        Assert.Equal(1, repository.PurgeCount);
    }
}
