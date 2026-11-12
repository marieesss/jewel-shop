using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.Entities;
using JewelryShop.Features.Charms.Repositories;
using JewelryShop.Features.Tags.Common;

namespace JewelryShop.API.Tests.Fakes;

public class FakeCharmRepository : InMemoryRepository<Charm>, ICharmRepository
{
    public long? LastFavoritesFirstForUserId { get; private set; }

    public TagFilter? LastTagFilter { get; private set; }

    public Task<Charm?> GetByIdWithTagsAsync(long id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    /// <summary>Renvoie les breloques semées sans filtrer ni trier : le tri est fait par EF.</summary>
    public Task<(IReadOnlyList<Charm> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        bool? inStockOnly,
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
        return Task.FromResult<(IReadOnlyList<Charm>, int)>((Entities.ToList(), Entities.Count));
    }
}
