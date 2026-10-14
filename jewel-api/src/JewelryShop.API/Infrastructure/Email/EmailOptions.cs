namespace JewelryShop.Infrastructure.Email;

/// <summary>
/// Configuration de l'envoi de mails (variables Email__*). Toutes les valeurs sont facultatives :
/// sans hôte SMTP, l'envoi n'est pas configuré et l'API démarre quand même.
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    public SmtpOptions Smtp { get; set; } = new();
    public string FromAddress { get; set; } = "no-reply@mycharmly.eu";
    public string FromName { get; set; } = "mycharmly";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Smtp.Host);
}

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1025;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; }

    public bool RequiresAuthentication => !string.IsNullOrWhiteSpace(Username);
}
