using System.Collections.Concurrent;
using JewelryShop.Common.Interfaces;
using JewelryShop.Common.Models;

namespace JewelryShop.API.IntegrationTests.Fakes;

/// <summary>Remplace l'envoi SMTP : aucun mail ne sort des tests, ceux reçus restent consultables.</summary>
public class FakeEmailSender : IEmailSender
{
    public ConcurrentQueue<EmailMessage> Sent { get; } = new();

    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        Sent.Enqueue(message);
        return Task.CompletedTask;
    }
}
