using System.Net;
using System.Net.Sockets;
using System.Text;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>
/// Client SMTP qui n'ouvre aucune connexion réseau : il enregistre les appels
/// et peut simuler un serveur injoignable.
/// </summary>
public class FakeSmtpClient : SmtpClient
{
    public bool FailOnConnect { get; init; }

    public string? ConnectedHost { get; private set; }
    public int ConnectedPort { get; private set; }
    public SecureSocketOptions? ConnectedSecureSocketOptions { get; private set; }
    public NetworkCredential? Credentials { get; private set; }
    public MimeMessage? SentMessage { get; private set; }
    public int ConnectCount { get; private set; }
    public int DisconnectCount { get; private set; }

    public override Task ConnectAsync(
        string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto,
        CancellationToken cancellationToken = default)
    {
        if (FailOnConnect)
            throw new SocketException((int)SocketError.HostUnreachable);

        ConnectedHost = host;
        ConnectedPort = port;
        ConnectedSecureSocketOptions = options;
        ConnectCount++;
        return Task.CompletedTask;
    }

    public override Task AuthenticateAsync(
        Encoding encoding, ICredentials credentials, CancellationToken cancellationToken = default)
    {
        Credentials = credentials.GetCredential(new Uri("smtp://localhost"), "PLAIN");
        return Task.CompletedTask;
    }

    public override Task<string> SendAsync(
        FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default,
        ITransferProgress? progress = null)
    {
        SentMessage = message;
        return Task.FromResult("250 OK");
    }

    public override Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
    {
        DisconnectCount++;
        return Task.CompletedTask;
    }
}
