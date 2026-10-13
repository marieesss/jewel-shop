using JewelryShop.Application.Common.Enums;
using JewelryShop.Application.Features.Chains;

namespace JewelryShop.API.Tests.Fakes;

public class FakeChainRepository : InMemoryRepository<Chain>, IChainRepository
{
    public Task<(IReadOnlyList<Chain> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
