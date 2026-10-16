using JewelryShop.Common.Models;

namespace JewelryShop.Features.Emails.Entities;

public class EmailOutboxMessage : BaseEntity
{
    public string ToAddress { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string HtmlBody { get; private set; } = null!;
    public string TextBody { get; private set; } = null!;
    public EmailOutboxStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public DateTime NextAttemptAt { get; private set; }
    public string? LastError { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    public static EmailOutboxMessage Create(string to, RenderedEmail email, DateTime now) => new()
    {
        ToAddress = to,
        Subject = email.Subject,
        HtmlBody = email.HtmlBody,
        TextBody = email.TextBody,
        Status = EmailOutboxStatus.Pending,
        NextAttemptAt = now,
        CreatedAt = now
    };

    public EmailMessage ToEmailMessage() => new(ToAddress, Subject, HtmlBody, TextBody);

    public void MarkSent(DateTime now)
    {
        Status = EmailOutboxStatus.Sent;
        SentAt = now;
        LastError = null;
    }

    public void RecordFailure(string error, DateTime now)
    {
        Attempts++;
        LastError = Truncate(error, EmailOutboxRetryPolicy.MaxErrorLength);

        if (HasExhaustedAttempts)
            Status = EmailOutboxStatus.Failed;
        else
            NextAttemptAt = now + EmailOutboxRetryPolicy.RetryDelays[Attempts - 1];
    }

    private bool HasExhaustedAttempts => Attempts >= EmailOutboxRetryPolicy.MaxAttempts;

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
