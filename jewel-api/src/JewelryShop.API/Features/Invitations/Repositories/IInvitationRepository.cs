using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Invitations.Repositories;

public interface IInvitationRepository : IRepository<Invitation>
{
    /// <summary>Ajoute l'invitation au contexte sans sauvegarder : elle part avec le SaveChanges de l'appelant.</summary>
    void Add(Invitation invitation);

    /// <summary>Invitations au statut Pending pour cet email, dépassées comprises, suivies par le contexte.</summary>
    Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(string email, CancellationToken ct);

    /// <summary>Invitation dont le token a ce hash, quel que soit son statut, suivie par le contexte.</summary>
    Task<Invitation?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);

    /// <summary>Passe à Expired les invitations Pending dont l'échéance est atteinte.</summary>
    Task<int> ExpirePendingAsync(CancellationToken ct);
}
