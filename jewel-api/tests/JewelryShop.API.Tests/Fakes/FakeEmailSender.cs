using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Application.Common.Models;

namespace JewelryShop.API.Tests.Fakes;

public class FakeEmailSender : IEmailSender
{
    private int _calls;

    public Exception? FailWith { get; init; }

    /// <summary>Nombre de premiers envois qui lèvent <see cref="FailWith"/> ; tous si null.</summary>
    public int? FailingCalls { get; init; }

    public List<EmailMessage> Sent { get; } = new();

    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        _calls++;
        if (FailWith is not null && (FailingCalls is null || _calls <= FailingCalls))
            throw FailWith;

        Sent.Add(message);
        return Task.CompletedTask;
    }
}
