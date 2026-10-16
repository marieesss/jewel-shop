using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Charms.Repositories;

public interface ICharmRepository : IRepository<Charm>
{
    Task<(IReadOnlyList<Charm> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        bool? inStockOnly,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
