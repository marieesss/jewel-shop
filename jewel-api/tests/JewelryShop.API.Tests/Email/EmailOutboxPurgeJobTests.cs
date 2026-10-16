using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Emails.Jobs;

namespace JewelryShop.API.Tests.Email;

public class EmailOutboxPurgeJobTests
{
    [Fact]
    public async Task Supprime_les_mails_envoyes_depuis_plus_de_30_jours()
    {
        var repository = new FakeEmailOutboxRepository();

        await new EmailOutboxPurgeJob(repository).RunAsync(CancellationToken.None);

        Assert.Equal(TimeSpan.FromDays(30), repository.PurgedRetention);
    }

    [Fact]
    public void S_execute_toutes_les_heures()
    {
        Assert.Equal(TimeSpan.FromHours(1), new EmailOutboxPurgeJob(new FakeEmailOutboxRepository()).Interval);
    }
}
