using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Chains.Repositories;

public interface IChainRepository : IRepository<Chain>
{
    /// <summary>Charge la chaîne avec ses rouleaux, nécessaires au stock renvoyé dans le DTO.</summary>
    Task<Chain?> GetByIdWithRollsAsync(long id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Chain> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
