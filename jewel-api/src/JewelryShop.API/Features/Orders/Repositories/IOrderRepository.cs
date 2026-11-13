using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Orders.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    /// <summary>Vrai si au moins une ligne de commande, quel que soit son statut, porte cette chaîne.</summary>
    Task<bool> ContainsChainAsync(long chainId, CancellationToken cancellationToken = default);

    /// <summary>Vrai si au moins une ligne de commande, quel que soit son statut, porte cette breloque.</summary>
    Task<bool> ContainsCharmAsync(long charmId, CancellationToken cancellationToken = default);
}
