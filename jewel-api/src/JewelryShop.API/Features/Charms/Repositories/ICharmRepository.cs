using JewelryShop.Application.Common.Persistence;

namespace JewelryShop.Application.Features.Charms;

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
