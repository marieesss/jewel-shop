using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Chains.Repositories;
using JewelryShop.Features.Tags.Common;

namespace JewelryShop.API.Tests.Fakes;

public class FakeChainRepository : InMemoryRepository<Chain>, IChainRepository
{
    public Task<Chain?> GetByIdWithRollsAndTagsAsync(long id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public long? LastFavoritesFirstForUserId { get; private set; }

    public TagFilter? LastTagFilter { get; private set; }

    /// <summary>Renvoie les chaînes semées sans filtrer ni trier : le tri est fait par EF.</summary>
    public Task<(IReadOnlyList<Chain> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        decimal? minPrice,
        decimal? maxPrice,
        TagFilter tagFilter,
        int page,
        int pageSize,
        long? favoritesFirstForUserId,
        CancellationToken cancellationToken = default)
    {
        LastFavoritesFirstForUserId = favoritesFirstForUserId;
        LastTagFilter = tagFilter;
        return Task.FromResult<(IReadOnlyList<Chain>, int)>((Entities.ToList(), Entities.Count));
    }
}
