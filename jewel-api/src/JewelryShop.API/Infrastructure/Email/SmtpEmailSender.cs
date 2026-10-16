using JewelryShop.Common.Interfaces;
using JewelryShop.Common.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JewelryShop.Infrastructure.Email;

/// <summary>
/// Envoi SMTP unique pour tous les environnements : Mailpit en local, relais SMTP du fournisseur
/// en staging et en production. Seule la configuration change.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        if (!_options.IsConfigured)
            throw new InvalidOperationException("Envoi de mails désactivé ou non configuré (Email__Enabled à false ou Email__Smtp__Host vide).");

        var mimeMessage = BuildMimeMessage(message);

        // Une connexion par envoi : les mails sont rares, une connexion persistante
        // devrait gérer les coupures du serveur pour un gain négligeable.
        using var client = CreateSmtpClient();
        await client.ConnectAsync(_options.Smtp.Host, _options.Smtp.Port, SecureSocketOption, ct);

        if (_options.Smtp.RequiresAuthentication)
            await client.AuthenticateAsync(_options.Smtp.Username, _options.Smtp.Password, ct);

        await client.SendAsync(mimeMessage, ct);
        await client.DisconnectAsync(quit: true, ct);
    }

    /// <summary>
    /// Point d'extension pour remplacer le client SMTP par une doublure dans les tests.
    /// </summary>
    protected virtual SmtpClient CreateSmtpClient() => new();

    private SecureSocketOptions SecureSocketOption =>
        _options.Smtp.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new BodyBuilder
        {
            TextBody = message.TextBody,
            HtmlBody = message.HtmlBody
        }.ToMessageBody();

        return mimeMessage;
    }
}
