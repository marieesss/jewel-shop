using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Chains.Repositories;

namespace JewelryShop.API.Tests.Fakes;

public class FakeChainRepository : InMemoryRepository<Chain>, IChainRepository
{
    public Task<Chain?> GetByIdWithRollsAsync(long id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<(IReadOnlyList<Chain> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
