using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Charms.Repositories;

public interface ICharmRepository : IRepository<Charm>
{
    /// <summary>
    /// Liste paginée, triée par id. Avec <paramref name="favoritesFirstForUserId"/>, les favoris
    /// de cet utilisateur passent d'abord, puis les autres, chaque groupe trié par id.
    /// </summary>
    Task<(IReadOnlyList<Charm> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        bool? inStockOnly,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        long? favoritesFirstForUserId,
        CancellationToken cancellationToken = default);
}
