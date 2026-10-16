using JewelryShop.Application.Common.Interfaces;

namespace JewelryShop.Application.Features.Emails.Jobs;

/// <summary>
/// Supprime les mails envoyés depuis plus de 30 jours. Les mails en échec définitif sont conservés.
/// </summary>
public class EmailOutboxPurgeJob : IPeriodicJob
{
    public static readonly TimeSpan SentRetention = TimeSpan.FromDays(30);

    private readonly IEmailOutboxRepository _repository;

    public EmailOutboxPurgeJob(IEmailOutboxRepository repository)
    {
        _repository = repository;
    }

    public TimeSpan Interval => TimeSpan.FromHours(1);

    public Task RunAsync(CancellationToken ct) => _repository.DeleteSentOlderThanAsync(SentRetention, ct);
}
