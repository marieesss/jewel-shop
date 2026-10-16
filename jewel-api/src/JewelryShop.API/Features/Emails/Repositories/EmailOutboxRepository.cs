using JewelryShop.Application.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Application.Features.Emails;

public class EmailOutboxRepository : Repository<EmailOutboxMessage>, IEmailOutboxRepository
{
    public EmailOutboxRepository(AppDbContext context) : base(context)
    {
    }

    public void Add(EmailOutboxMessage message) => DbSet.Add(message);

    public async Task<IReadOnlyList<EmailOutboxMessage>> LockDueBatchAsync(int batchSize, CancellationToken ct)
        => await DbSet
            .FromSql($"""
                SELECT * FROM email_outbox
                WHERE status = 'Pending' AND next_attempt_at <= NOW()
                ORDER BY id
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(ct);

    public Task InTransactionAsync(Func<Task> work, CancellationToken ct)
    {
        // EnableRetryOnFailure interdit une transaction ouverte à la main hors de la stratégie
        // d'exécution. Si la stratégie rejoue le bloc, on repart d'un contexte vide.
        var strategy = Context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            Context.ChangeTracker.Clear();
            await using var transaction = await Context.Database.BeginTransactionAsync(ct);
            await work();
            await transaction.CommitAsync(ct);
        });
    }

    public Task<int> DeleteSentOlderThanAsync(TimeSpan retention, CancellationToken ct)
        => Context.Database.ExecuteSqlAsync($"""
            DELETE FROM email_outbox
            WHERE status = 'Sent' AND sent_at < NOW() - {retention}
            """, ct);
}
