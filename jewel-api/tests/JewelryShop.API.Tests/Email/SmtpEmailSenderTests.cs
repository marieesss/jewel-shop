using System.Net.Sockets;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Common.Models;
using JewelryShop.Infrastructure.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JewelryShop.API.Tests.Email;

public class SmtpEmailSenderTests
{
    private static readonly EmailMessage _message =
        new("cliente@example.com", "Bienvenue", "<p>Bonjour</p>", "Bonjour");

    private static EmailOptions BuildOptions(Action<SmtpOptions>? configureSmtp = null)
    {
        var options = new EmailOptions { Smtp = { Host = "mailpit", Port = 1025 } };
        configureSmtp?.Invoke(options.Smtp);
        return options;
    }

    [Fact]
    public async Task Le_message_contient_l_expediteur_le_destinataire_et_le_sujet()
    {
        var client = new FakeSmtpClient();

        await new TestableSmtpEmailSender(BuildOptions(), client).SendAsync(_message, CancellationToken.None);

        var sent = client.SentMessage!;
        var from = Assert.Single(sent.From.Mailboxes);
        Assert.Equal("mycharmly", from.Name);
        Assert.Equal("no-reply@mycharmly.eu", from.Address);
        Assert.Equal("cliente@example.com", Assert.Single(sent.To.Mailboxes).Address);
        Assert.Equal("Bienvenue", sent.Subject);
    }

    [Fact]
    public async Task Le_corps_contient_une_partie_texte_puis_une_partie_html()
    {
        var client = new FakeSmtpClient();

        await new TestableSmtpEmailSender(BuildOptions(), client).SendAsync(_message, CancellationToken.None);

        var sent = client.SentMessage!;
        var body = Assert.IsType<MultipartAlternative>(sent.Body);
        Assert.Collection(body,
            part => Assert.Equal("text/plain", part.ContentType.MimeType),
            part => Assert.Equal("text/html", part.ContentType.MimeType));
        Assert.Equal("Bonjour", sent.TextBody?.Trim());
        Assert.Equal("<p>Bonjour</p>", sent.HtmlBody?.Trim());
    }

    [Fact]
    public async Task Se_connecte_a_l_hote_et_au_port_configures()
    {
        var client = new FakeSmtpClient();

        await new TestableSmtpEmailSender(BuildOptions(), client).SendAsync(_message, CancellationToken.None);

        Assert.Equal("mailpit", client.ConnectedHost);
        Assert.Equal(1025, client.ConnectedPort);
    }

    [Theory]
    [InlineData(true, SecureSocketOptions.StartTls)]
    [InlineData(false, SecureSocketOptions.None)]
    public async Task Utilise_StartTls_si_UseSsl_et_aucune_securite_sinon(bool useSsl, SecureSocketOptions expected)
    {
        var client = new FakeSmtpClient();

        await new TestableSmtpEmailSender(BuildOptions(smtp => smtp.UseSsl = useSsl), client)
            .SendAsync(_message, CancellationToken.None);

        Assert.Equal(expected, client.ConnectedSecureSocketOptions);
    }

    [Fact]
    public async Task N_authentifie_pas_quand_l_identifiant_est_vide()
    {
        var client = new FakeSmtpClient();

        await new TestableSmtpEmailSender(BuildOptions(smtp => smtp.Username = string.Empty), client)
            .SendAsync(_message, CancellationToken.None);

        Assert.Null(client.Credentials);
    }

    [Fact]
    public async Task S_authentifie_quand_l_identifiant_est_renseigne()
    {
        var client = new FakeSmtpClient();
        var options = BuildOptions(smtp =>
        {
            smtp.Username = "user";
            smtp.Password = "secret";
        });

        await new TestableSmtpEmailSender(options, client).SendAsync(_message, CancellationToken.None);

        Assert.Equal("user", client.Credentials!.UserName);
        Assert.Equal("secret", client.Credentials.Password);
    }

    [Fact]
    public async Task Propage_l_exception_quand_le_serveur_est_injoignable()
    {
        var client = new FakeSmtpClient { FailOnConnect = true };
        var sender = new TestableSmtpEmailSender(BuildOptions(), client);

        await Assert.ThrowsAsync<SocketException>(() => sender.SendAsync(_message, CancellationToken.None));
        Assert.Null(client.SentMessage);
    }

    [Fact]
    public async Task Refuse_d_envoyer_quand_l_hote_n_est_pas_configure()
    {
        var client = new FakeSmtpClient();
        var sender = new TestableSmtpEmailSender(BuildOptions(smtp => smtp.Host = string.Empty), client);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync(_message, CancellationToken.None));
        Assert.Equal(0, client.ConnectCount);
    }

    [Fact]
    public async Task Ouvre_et_ferme_une_connexion_a_chaque_envoi()
    {
        var clients = new List<FakeSmtpClient>();
        var sender = new TestableSmtpEmailSender(BuildOptions(), () =>
        {
            var client = new FakeSmtpClient();
            clients.Add(client);
            return client;
        });

        await sender.SendAsync(_message, CancellationToken.None);
        await sender.SendAsync(_message, CancellationToken.None);

        Assert.Equal(2, clients.Count);
        Assert.All(clients, client =>
        {
            Assert.Equal(1, client.ConnectCount);
            Assert.Equal(1, client.DisconnectCount);
        });
    }

    private class TestableSmtpEmailSender(EmailOptions options, Func<SmtpClient> createClient)
        : SmtpEmailSender(Options.Create(options))
    {
        public TestableSmtpEmailSender(EmailOptions options, SmtpClient client)
            : this(options, () => client)
        {
        }

        protected override SmtpClient CreateSmtpClient() => createClient();
    }
}
