using JewelryShop.Common.Interfaces;
using JewelryShop.Infrastructure.Email;
using Microsoft.Extensions.Options;

namespace JewelryShop.Features.Emails.Jobs;

public class EmailOutboxDispatchJob : IPeriodicJob
{
    public const int BatchSize = 20;

    private readonly IEmailOutboxRepository _repository;
    private readonly IEmailSender _sender;
    private readonly EmailOptions _emailOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<EmailOutboxDispatchJob> _logger;

    public EmailOutboxDispatchJob(
        IEmailOutboxRepository repository,
        IEmailSender sender,
        IOptions<EmailOptions> emailOptions,
        TimeProvider timeProvider,
        ILogger<EmailOutboxDispatchJob> logger)
    {
        _repository = repository;
        _sender = sender;
        _emailOptions = emailOptions.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public TimeSpan Interval => TimeSpan.FromSeconds(10);

    public async Task RunAsync(CancellationToken ct)
    {
        // Envoi désactivé ou sans SMTP : les mails restent en attente sans consommer de tentative :
        // ils partiront dès que l'envoi sera configuré.
        if (!_emailOptions.IsConfigured)
            return;

        await _repository.InTransactionAsync(async () =>
        {
            var messages = await _repository.LockDueBatchAsync(BatchSize, ct);
            foreach (var message in messages)
                await SendAsync(message, ct);

            await _repository.SaveChangesAsync(ct);
        }, ct);
    }

    private async Task SendAsync(EmailOutboxMessage message, CancellationToken ct)
    {
        try
        {
            await _sender.SendAsync(message.ToEmailMessage(), ct);
            message.MarkSent(UtcNow);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            message.RecordFailure(ex.Message, UtcNow);
            if (message.Status == EmailOutboxStatus.Failed)
                _logger.LogWarning(
                    "Mail {EmailId} abandonné après {Attempts} tentatives.", message.Id, message.Attempts);
        }
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
