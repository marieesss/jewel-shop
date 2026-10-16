using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Auth.Repositories;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    /// <summary>Ajoute le token au contexte sans sauvegarder : il part avec le SaveChanges de l'appelant.</summary>
    void Add(PasswordResetToken token);

    /// <summary>Token suivi par le contexte, avec son utilisateur.</summary>
    Task<PasswordResetToken?> GetByHashWithUserAsync(string tokenHash, CancellationToken ct);

    /// <summary>Tokens pas encore utilisés de l'utilisateur, expirés compris, suivis par le contexte.</summary>
    Task<IReadOnlyList<PasswordResetToken>> GetUnusedByUserIdAsync(long userId, CancellationToken ct);

    Task<int> DeleteExpiredOrUsedAsync(CancellationToken ct);
}
