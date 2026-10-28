namespace JewelryShop.Features.Auth.Seeding;

/// <summary>
/// Identifiants du compte admin créé au démarrage en développement (variables DevAdmin__*).
/// Facultatives : vides, aucun compte n'est créé.
/// </summary>
public class DevAdminOptions
{
    public const string SectionName = "DevAdmin";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}
