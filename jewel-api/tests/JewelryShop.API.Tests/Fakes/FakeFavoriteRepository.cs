using JewelryShop.Features.Favorites.Entities;
using JewelryShop.Features.Favorites.Repositories;

namespace JewelryShop.API.Tests.Fakes;

public class FakeFavoriteRepository : InMemoryRepository<Favorite>, IFavoriteRepository
{
    public Task<(IReadOnlyList<Favorite> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<bool> ExistsAsync(
        long userId, long? chainId, long? charmId, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.Any(
            f => f.UserId == userId && f.ChainId == chainId && f.CharmId == charmId));
}
