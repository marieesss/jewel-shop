using JewelryShop.Application.Common.Models;
using JewelryShop.Application.Features.Emails;

namespace JewelryShop.API.Tests.Email;

public class EmailOutboxMessageTests
{
    private static readonly DateTime _now = new(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc);

    private static EmailOutboxMessage CreatePendingMessage()
        => EmailOutboxMessage.Create(
            "cliente@example.com", new RenderedEmail("Bienvenue", "<p>Bonjour</p>", "Bonjour"), _now);

    private static EmailOutboxMessage CreateMessageWithFailures(int failures)
    {
        var message = CreatePendingMessage();
        for (var i = 0; i < failures; i++)
            message.RecordFailure("Serveur injoignable", _now);
        return message;
    }

    [Fact]
    public void Un_nouveau_mail_est_en_attente_et_a_envoyer_tout_de_suite()
    {
        var message = CreatePendingMessage();

        Assert.Equal(EmailOutboxStatus.Pending, message.Status);
        Assert.Equal(0, message.Attempts);
        Assert.Equal(_now, message.NextAttemptAt);
        Assert.Equal(_now, message.CreatedAt);
        Assert.Null(message.SentAt);
        Assert.Equal(
            new EmailMessage("cliente@example.com", "Bienvenue", "<p>Bonjour</p>", "Bonjour"),
            message.ToEmailMessage());
    }

    [Fact]
    public void Un_envoi_reussi_passe_le_mail_a_envoye_et_efface_la_derniere_erreur()
    {
        var message = CreateMessageWithFailures(1);
        var sentAt = _now.AddMinutes(1);

        message.MarkSent(sentAt);

        Assert.Equal(EmailOutboxStatus.Sent, message.Status);
        Assert.Equal(sentAt, message.SentAt);
        Assert.Null(message.LastError);
    }

    [Fact]
    public void Un_premier_echec_programme_une_nouvelle_tentative_une_minute_plus_tard()
    {
        var message = CreatePendingMessage();

        message.RecordFailure("Serveur injoignable", _now);

        Assert.Equal(EmailOutboxStatus.Pending, message.Status);
        Assert.Equal(1, message.Attempts);
        Assert.Equal("Serveur injoignable", message.LastError);
        Assert.Equal(_now.AddMinutes(1), message.NextAttemptAt);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 5)]
    [InlineData(3, 15)]
    [InlineData(4, 60)]
    [InlineData(5, 240)]
    public void Le_delai_avant_la_tentative_suivante_depend_du_nombre_d_echecs(int failures, int expectedDelayInMinutes)
    {
        var message = CreateMessageWithFailures(failures);

        Assert.Equal(EmailOutboxStatus.Pending, message.Status);
        Assert.Equal(_now.AddMinutes(expectedDelayInMinutes), message.NextAttemptAt);
    }

    [Fact]
    public void Le_sixieme_echec_passe_le_mail_en_echec_definitif()
    {
        var message = CreateMessageWithFailures(5);

        message.RecordFailure("Serveur injoignable", _now);

        Assert.Equal(EmailOutboxStatus.Failed, message.Status);
        Assert.Equal(6, message.Attempts);
    }

    [Fact]
    public void La_derniere_erreur_est_tronquee_a_2000_caracteres()
    {
        var message = CreatePendingMessage();

        message.RecordFailure(new string('x', 2500), _now);

        Assert.Equal(2000, message.LastError!.Length);
    }
}
