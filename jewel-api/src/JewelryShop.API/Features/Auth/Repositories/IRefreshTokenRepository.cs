using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Auth.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>Ajoute le token au contexte sans sauvegarder : il part avec le SaveChanges de l'appelant.</summary>
    void Add(RefreshToken token);

    /// <summary>Token suivi par le contexte, avec son utilisateur.</summary>
    Task<RefreshToken?> GetByHashWithUserAsync(string tokenHash, CancellationToken ct);

    /// <summary>Tokens ni tournés, ni révoqués, ni expirés à <paramref name="now"/>, suivis par le contexte.</summary>
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(long userId, DateTime now, CancellationToken ct);

    Task<int> DeleteExpiredAsync(CancellationToken ct);
}
