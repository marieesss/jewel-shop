using JewelryShop.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Features.Chains.Repositories;

public class ChainRepository : Repository<Chain>, IChainRepository
{
    public ChainRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Chain?> GetByIdWithRollsAndTagsAsync(long id, CancellationToken cancellationToken = default)
        => await WithRollsAndTags(DbSet)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Chain> Items, int TotalCount)> GetPagedAsync(
        ProductColor? color,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        long? favoritesFirstForUserId,
        CancellationToken cancellationToken = default)
    {
        var query = WithRollsAndTags(DbSet.AsNoTracking());

        if (color.HasValue)
            query = query.Where(c => c.Color == color.Value);

        if (minPrice.HasValue)
            query = query.Where(c => (c.Price ?? c.PricePerMeter) >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(c => (c.Price ?? c.PricePerMeter) <= maxPrice.Value);

        query = favoritesFirstForUserId is { } userId
            ? query.OrderByDescending(c => c.Favorites.Any(f => f.UserId == userId)).ThenBy(c => c.Id)
            : query.OrderBy(c => c.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    // Deux collections incluses dans une seule requête multiplieraient les lignes (rouleaux × tags) :
    // chacune est chargée par sa propre requête, une fois pour toute la page.
    private static IQueryable<Chain> WithRollsAndTags(IQueryable<Chain> chains)
        => chains
            .Include(c => c.Rolls)
            .Include(c => c.ChainTags).ThenInclude(ct => ct.Tag)
            .AsSplitQuery();
}
