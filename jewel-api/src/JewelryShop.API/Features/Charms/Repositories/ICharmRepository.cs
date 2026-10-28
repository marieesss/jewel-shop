using JewelryShop.Common.Persistence;
using JewelryShop.Features.Tags.Common;

namespace JewelryShop.Features.Charms.Repositories;

public interface ICharmRepository : IRepository<Charm>
{
    /// <summary>Charge la breloque avec ses tags, renvoyés dans le DTO.</summary>
    Task<Charm?> GetByIdWithTagsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste paginée, triée par id, avec les tags de chaque breloque. Avec <paramref name="favoritesFirstForUserId"/>, les favoris
    /// de cet utilisateur passent d'abord, puis les autres, chaque groupe trié par id. Seules les breloques qui portent
    /// un tag de chaque groupe de <paramref name="tagFilter"/> sont renvoyées.
    /// </summary>
    Task<(IReadOnlyList<Charm> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        bool? inStockOnly,
        decimal? minPrice,
        decimal? maxPrice,
        TagFilter tagFilter,
        int page,
        int pageSize,
        long? favoritesFirstForUserId,
        CancellationToken cancellationToken = default);
}
