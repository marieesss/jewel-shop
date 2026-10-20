using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.Entities;
using JewelryShop.Features.Charms.Repositories;

namespace JewelryShop.API.Tests.Fakes;

public class FakeCharmRepository : InMemoryRepository<Charm>, ICharmRepository
{
    public long? LastFavoritesFirstForUserId { get; private set; }

    /// <summary>Renvoie les breloques semées sans filtrer ni trier : le tri est fait par EF.</summary>
    public Task<(IReadOnlyList<Charm> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        bool? inStockOnly,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        long? favoritesFirstForUserId,
        CancellationToken cancellationToken = default)
    {
        LastFavoritesFirstForUserId = favoritesFirstForUserId;
        return Task.FromResult<(IReadOnlyList<Charm>, int)>((Entities.ToList(), Entities.Count));
    }
}
