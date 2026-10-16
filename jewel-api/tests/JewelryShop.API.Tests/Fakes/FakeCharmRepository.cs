using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.Entities;
using JewelryShop.Features.Charms.Repositories;

namespace JewelryShop.API.Tests.Fakes;

public class FakeCharmRepository : InMemoryRepository<Charm>, ICharmRepository
{
    public Task<(IReadOnlyList<Charm> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        bool? inStockOnly,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
