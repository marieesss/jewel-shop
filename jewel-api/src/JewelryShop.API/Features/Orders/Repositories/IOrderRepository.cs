using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Orders.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    /// <summary>Vrai si au moins une ligne de commande, quel que soit son statut, porte cette chaîne.</summary>
    Task<bool> ContainsChainAsync(long chainId, CancellationToken cancellationToken = default);

    /// <summary>Vrai si au moins une ligne de commande, quel que soit son statut, porte cette breloque.</summary>
    Task<bool> ContainsCharmAsync(long charmId, CancellationToken cancellationToken = default);

    /// <summary>Commande avec ses lignes et son historique, en lecture seule.</summary>
    Task<Order?> GetByIdWithDetailsAsNoTrackingAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Commandes de l'utilisateur, des plus récentes aux plus anciennes, sans leurs lignes.</summary>
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exécute <paramref name="work"/> dans une transaction : tout est annulé si une étape lève une exception.
    /// Le contexte est vidé au début du bloc, qui doit donc recharger les entités qu'il modifie.
    /// </summary>
    Task InTransactionAsync(Func<Task> work, CancellationToken cancellationToken);

    /// <summary>
    /// Retire la longueur du plus petit rouleau de la chaîne qui la contient encore, sans jamais rendre
    /// un rouleau négatif. Renvoie l'id du rouleau entamé, ou null si aucun rouleau ne suffit.
    /// </summary>
    Task<long?> TakeChainLengthAsync(long chainId, int lengthCm, CancellationToken cancellationToken);

    /// <summary>Retire la quantité du stock de la breloque si elle y est encore. Faux sinon, sans rien modifier.</summary>
    Task<bool> TakeCharmStockAsync(long charmId, int quantity, CancellationToken cancellationToken);

    /// <summary>Valeur suivante de la séquence des numéros de commande.</summary>
    Task<long> NextNumberSequenceValueAsync(CancellationToken cancellationToken);
}
