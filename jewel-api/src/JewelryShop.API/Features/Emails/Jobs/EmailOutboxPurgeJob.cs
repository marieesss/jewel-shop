using Coravel.Invocable;

namespace JewelryShop.Features.Emails.Jobs;

/// <summary>
/// Supprime les mails envoyés depuis plus de 30 jours. Les mails en échec définitif sont conservés.
/// </summary>
public class EmailOutboxPurgeJob : IInvocable, ICancellableInvocable
{
    public static readonly TimeSpan SentRetention = TimeSpan.FromDays(30);

    private readonly IEmailOutboxRepository _repository;

    public EmailOutboxPurgeJob(IEmailOutboxRepository repository)
    {
        _repository = repository;
    }

    public CancellationToken CancellationToken { get; set; }

    public Task Invoke() => _repository.DeleteSentOlderThanAsync(SentRetention, CancellationToken);
}
