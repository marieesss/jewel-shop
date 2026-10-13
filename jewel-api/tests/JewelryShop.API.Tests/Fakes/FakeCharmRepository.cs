using JewelryShop.Application.Common.Enums;
using JewelryShop.Application.Features.Charms;

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
