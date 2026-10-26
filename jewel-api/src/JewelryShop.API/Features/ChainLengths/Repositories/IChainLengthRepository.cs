using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.ChainLengths.Repositories;

public interface IChainLengthRepository : IRepository<ChainLength>
{
    /// <summary>Toutes les longueurs, de la plus courte à la plus longue.</summary>
    Task<IReadOnlyList<ChainLength>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

    /// <summary>Vrai si une autre longueur que <paramref name="excludedId"/> a déjà cette valeur en cm.</summary>
    Task<bool> ExistsByLengthAsync(int lengthCm, long? excludedId = null, CancellationToken cancellationToken = default);
}
