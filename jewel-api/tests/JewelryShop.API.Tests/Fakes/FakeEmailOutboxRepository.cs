using JewelryShop.Application.Features.Emails;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>
/// Le verrouillage et le filtre des mails à envoyer sont faits en SQL : ici, les mails
/// renvoyés par LockDueBatchAsync sont simplement ceux qui ont été ajoutés.
/// </summary>
public class FakeEmailOutboxRepository : InMemoryRepository<EmailOutboxMessage>, IEmailOutboxRepository
{
    public int TransactionCount { get; private set; }
    public TimeSpan? PurgedRetention { get; private set; }

    public void Add(EmailOutboxMessage message) => Entities.Add(message);

    public Task<IReadOnlyList<EmailOutboxMessage>> LockDueBatchAsync(int batchSize, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<EmailOutboxMessage>>(Entities.Take(batchSize).ToList());

    public async Task InTransactionAsync(Func<Task> work, CancellationToken ct)
    {
        TransactionCount++;
        await work();
    }

    public Task<int> DeleteSentOlderThanAsync(TimeSpan retention, CancellationToken ct)
    {
        PurgedRetention = retention;
        return Task.FromResult(0);
    }
}
