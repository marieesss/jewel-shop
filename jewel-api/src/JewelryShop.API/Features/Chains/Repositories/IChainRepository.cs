using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Chains.Repositories;

public interface IChainRepository : IRepository<Chain>
{
    Task<(IReadOnlyList<Chain> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
