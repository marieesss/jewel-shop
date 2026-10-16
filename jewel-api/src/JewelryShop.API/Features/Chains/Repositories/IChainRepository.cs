using JewelryShop.Application.Common.Persistence;

namespace JewelryShop.Application.Features.Chains;

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
