using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Favorites.Repositories;

public interface IFavoriteRepository : IRepository<Favorite>
{
    Task<(IReadOnlyList<Favorite> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        long userId, long? chainId, long? charmId, CancellationToken cancellationToken = default);
}
