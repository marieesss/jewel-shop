using System.Net.Sockets;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Common.Models;
using JewelryShop.Application.Features.Emails;
using JewelryShop.Application.Features.Emails.Jobs;
using JewelryShop.Infrastructure.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JewelryShop.API.Tests.Email;

public class EmailOutboxDispatchJobTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 15, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeEmailOutboxRepository _repository = new();
    private readonly ListLogger<EmailOutboxDispatchJob> _logger = new();

    private EmailOutboxDispatchJob BuildJob(FakeEmailSender sender, string smtpHost = "mailpit", bool enabled = true)
        => new(
            _repository,
            sender,
            Options.Create(new EmailOptions { Enabled = enabled, Smtp = { Host = smtpHost } }),
            new FakeTimeProvider(_now),
            _logger);

    private EmailOutboxMessage SeedPendingMessage(int previousFailures = 0)
    {
        var message = EmailOutboxMessage.Create(
            "cliente@example.com",
            new RenderedEmail("Bienvenue", "<p>Bonjour</p>", "Bonjour"),
            _now.UtcDateTime.AddHours(-1));
        for (var i = 0; i < previousFailures; i++)
            message.RecordFailure("Serveur injoignable", _now.UtcDateTime.AddHours(-1));
        _repository.Add(message);
        return message;
    }

    private static FakeEmailSender UnreachableSender()
        => new() { FailWith = new SocketException((int)SocketError.HostUnreachable) };

    [Fact]
    public async Task Envoie_le_mail_en_attente_et_le_passe_a_envoye()
    {
        var message = SeedPendingMessage();
        var sender = new FakeEmailSender();

        await BuildJob(sender).RunAsync(CancellationToken.None);

        Assert.Equal(message.ToEmailMessage(), Assert.Single(sender.Sent));
        Assert.Equal(EmailOutboxStatus.Sent, message.Status);
        Assert.Equal(_now.UtcDateTime, message.SentAt);
    }

    [Fact]
    public async Task Sauvegarde_les_changements_dans_une_transaction()
    {
        SeedPendingMessage();

        await BuildJob(new FakeEmailSender()).RunAsync(CancellationToken.None);

        Assert.Equal(1, _repository.TransactionCount);
        Assert.Equal(1, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task Un_serveur_injoignable_programme_une_nouvelle_tentative_une_minute_plus_tard()
    {
        var message = SeedPendingMessage();

        await BuildJob(UnreachableSender()).RunAsync(CancellationToken.None);

        Assert.Equal(EmailOutboxStatus.Pending, message.Status);
        Assert.Equal(1, message.Attempts);
        Assert.False(string.IsNullOrEmpty(message.LastError));
        Assert.Equal(_now.UtcDateTime.AddMinutes(1), message.NextAttemptAt);
        Assert.Equal(1, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task Un_mail_qui_echoue_une_sixieme_fois_passe_en_echec_definitif()
    {
        var message = SeedPendingMessage(previousFailures: 5);

        await BuildJob(UnreachableSender()).RunAsync(CancellationToken.None);

        Assert.Equal(EmailOutboxStatus.Failed, message.Status);
        Assert.Equal(6, message.Attempts);
    }

    [Fact]
    public async Task Le_passage_en_echec_definitif_est_journalise_en_warning_sans_le_contenu()
    {
        var message = SeedPendingMessage(previousFailures: 5);
        message.Id = 4242;

        await BuildJob(UnreachableSender()).RunAsync(CancellationToken.None);

        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("4242", entry.Message);
        Assert.DoesNotContain(message.ToAddress, entry.Message);
        Assert.DoesNotContain(message.Subject, entry.Message);
    }

    [Fact]
    public async Task Un_echec_qui_laisse_des_tentatives_n_est_pas_journalise()
    {
        SeedPendingMessage();

        await BuildJob(UnreachableSender()).RunAsync(CancellationToken.None);

        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task L_echec_d_un_mail_n_empeche_pas_l_envoi_des_suivants()
    {
        var failing = SeedPendingMessage();
        var next = SeedPendingMessage();
        var sender = new FakeEmailSender
        {
            FailWith = new SocketException((int)SocketError.HostUnreachable),
            FailingCalls = 1
        };

        await BuildJob(sender).RunAsync(CancellationToken.None);

        Assert.Equal(EmailOutboxStatus.Pending, failing.Status);
        Assert.Equal(EmailOutboxStatus.Sent, next.Status);
    }

    [Fact]
    public async Task Ne_fait_rien_quand_l_hote_smtp_n_est_pas_configure()
    {
        var message = SeedPendingMessage();
        var sender = new FakeEmailSender();

        await BuildJob(sender, smtpHost: string.Empty).RunAsync(CancellationToken.None);

        Assert.Empty(sender.Sent);
        Assert.Equal(EmailOutboxStatus.Pending, message.Status);
        Assert.Equal(0, message.Attempts);
        Assert.Equal(0, _repository.TransactionCount);
    }

    [Fact]
    public async Task Ne_fait_rien_quand_l_envoi_est_desactive()
    {
        var message = SeedPendingMessage();
        var sender = new FakeEmailSender();

        await BuildJob(sender, enabled: false).RunAsync(CancellationToken.None);

        Assert.Empty(sender.Sent);
        Assert.Equal(EmailOutboxStatus.Pending, message.Status);
        Assert.Equal(0, message.Attempts);
        Assert.Equal(0, _repository.TransactionCount);
    }
}
