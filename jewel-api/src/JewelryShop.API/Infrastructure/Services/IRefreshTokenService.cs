using JewelryShop.Common.Exceptions;

namespace JewelryShop.Infrastructure.Services;

public interface IRefreshTokenService
{
    /// <summary>
    /// Crée un token valable <see cref="RefreshTokenDefaults.Lifetime"/> et renvoie sa valeur en clair.
    /// Ne sauvegarde pas : c'est l'appelant qui appelle SaveChangesAsync.
    /// </summary>
    string Issue(User user);

    /// <summary>
    /// Remplace un token actif par un nouveau et sauvegarde. Un token déjà tourné est une réutilisation :
    /// toutes les sessions de l'utilisateur sont révoquées.
    /// </summary>
    /// <exception cref="UnauthorizedException">Token inconnu, expiré, révoqué ou réutilisé.</exception>
    Task<(User User, string NewToken)> RotateAsync(string token, CancellationToken ct);

    /// <summary>Révoque le token s'il est actif et sauvegarde. Sinon, ne fait rien.</summary>
    Task RevokeAsync(string token, CancellationToken ct);

    /// <summary>Révoque tous les tokens actifs de l'utilisateur, sans sauvegarder.</summary>
    Task RevokeAllForUserAsync(long userId, CancellationToken ct);
}
