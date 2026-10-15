namespace JewelryShop.Infrastructure.Email;

/// <summary>
/// Configuration de l'envoi de mails (variables Email__*). Toutes les valeurs sont facultatives :
/// sans Email__Enabled à true ou sans hôte SMTP, l'envoi n'est pas configuré et l'API démarre quand même.
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>
    /// Désactivé par défaut : en staging et en production, le déploiement ne l'active que si les
    /// identifiants SMTP sont fournis, pour ne jamais tenter d'envoi sans authentification.
    /// </summary>
    public bool Enabled { get; set; }

    public SmtpOptions Smtp { get; set; } = new();
    public string FromAddress { get; set; } = "no-reply@mycharmly.eu";
    public string FromName { get; set; } = "mycharmly";

    public bool IsConfigured => Enabled && !string.IsNullOrWhiteSpace(Smtp.Host);
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
