using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Chains.Repositories;

public interface IChainRepository : IRepository<Chain>
{
    /// <summary>Charge la chaîne avec ses rouleaux (pour le stock) et ses tags, renvoyés dans le DTO.</summary>
    Task<Chain?> GetByIdWithRollsAndTagsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liste paginée, triée par id, avec les rouleaux et les tags de chaque chaîne. Avec <paramref name="favoritesFirstForUserId"/>, les favoris
    /// de cet utilisateur passent d'abord, puis les autres, chaque groupe trié par id.
    /// </summary>
    Task<(IReadOnlyList<Chain> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        long? favoritesFirstForUserId,
        CancellationToken cancellationToken = default);
}
