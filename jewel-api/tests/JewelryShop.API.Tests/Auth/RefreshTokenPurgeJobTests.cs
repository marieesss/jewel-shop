using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Features.Auth.Jobs;

namespace JewelryShop.API.Tests.Auth;

public class RefreshTokenPurgeJobTests
{
    [Fact]
    public async Task Supprime_les_refresh_tokens_expires()
    {
        var repository = new FakeRefreshTokenRepository();

        await new RefreshTokenPurgeJob(repository).RunAsync(CancellationToken.None);

        Assert.Equal(1, repository.PurgeCount);
    }

    [Fact]
    public void S_execute_toutes_les_heures()
    {
        Assert.Equal(TimeSpan.FromHours(1), new RefreshTokenPurgeJob(new FakeRefreshTokenRepository()).Interval);
    }
}
