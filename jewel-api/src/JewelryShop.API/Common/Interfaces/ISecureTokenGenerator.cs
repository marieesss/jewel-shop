namespace JewelryShop.Application.Common.Interfaces;

/// <summary>
/// Tokens opaques (refresh, réinitialisation, invitation). Seul le hash est stocké :
/// le token en clair n'est jamais enregistré ni journalisé.
/// </summary>
public interface ISecureTokenGenerator
{
    /// <summary>Token aléatoire de 32 octets, en base64url sans padding (43 caractères).</summary>
    string Generate();

    /// <summary>SHA-256 du token (UTF-8), en hexadécimal minuscule (64 caractères).</summary>
    string Hash(string token);
}
