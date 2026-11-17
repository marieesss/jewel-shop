namespace JewelryShop.Infrastructure.Email;

/// <summary>
/// Noms des templates de mail. Chaque nom correspond à trois ressources embarquées dans
/// Templates/ : &lt;Nom&gt;.subject.sbn, &lt;Nom&gt;.html.sbn et &lt;Nom&gt;.txt.sbn.
/// </summary>
public static class EmailTemplates
{
    /// <summary>Template utilisé seulement par les tests du renderer.</summary>
    public const string Test = "Test";

    /// <summary>Lien de réinitialisation du mot de passe. Modèle : FirstName, ResetUrl.</summary>
    public const string PasswordReset = "PasswordReset";

    /// <summary>
    /// Lien d'inscription envoyé par un admin. Modèle : InviterName (null si l'admin n'a pas de nom),
    /// RegisterUrl, ExpiresAt (date déjà formatée en dd/MM/yyyy).
    /// </summary>
    public const string Invitation = "Invitation";

    /// <summary>
    /// Confirmation d'une nouvelle commande au client. Modèle : FirstName, Order (<c>OrderEmailSummary</c>),
    /// OrderUrl.
    /// </summary>
    public const string OrderConfirmation = "OrderConfirmation";

    /// <summary>
    /// Avis de nouvelle commande à un admin. Modèle : CustomerName, CustomerEmail, Order (<c>OrderEmailSummary</c>),
    /// OrderUrl (page admin de la commande).
    /// </summary>
    public const string OrderCreatedAdmin = "OrderCreatedAdmin";

    /// <summary>
    /// Avis à un admin qu'un client a annulé sa commande. Modèle : CustomerName, CustomerEmail,
    /// Order (<c>OrderEmailSummary</c>), OrderUrl (page admin de la commande).
    /// </summary>
    public const string OrderCancelledByCustomerAdmin = "OrderCancelledByCustomerAdmin";

    /// <summary>
    /// Avis à un admin qu'un client a reçu sa commande. Modèle : CustomerName, CustomerEmail,
    /// Order (<c>OrderEmailSummary</c>), OrderUrl (page admin de la commande).
    /// </summary>
    public const string OrderReceivedAdmin = "OrderReceivedAdmin";
}
