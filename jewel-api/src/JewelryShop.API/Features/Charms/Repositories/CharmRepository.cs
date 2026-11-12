using JewelryShop.Common.Persistence;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Features.Charms.Repositories;

public class CharmRepository : Repository<Charm>, ICharmRepository
{
    public CharmRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Charm?> GetByIdWithTagsAsync(long id, CancellationToken cancellationToken = default)
        => await WithTags(DbSet).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Charm> Items, int TotalCount)> GetPagedAsync(
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
        var query = WithTags(DbSet.AsNoTracking());

        if (color.HasValue)
            query = query.Where(c => c.Color == color.Value);

        if (inStockOnly == true)
            query = query.Where(c => c.Stock > 0);

        if (minPrice.HasValue)
            query = query.Where(c => c.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(c => c.Price <= maxPrice.Value);

        foreach (var tagIds in tagFilter.TagIdGroups)
            query = query.Where(c => c.CharmTags.Any(ct => tagIds.Contains(ct.TagId)));

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

    private static IQueryable<Charm> WithTags(IQueryable<Charm> charms)
        => charms.Include(c => c.CharmTags).ThenInclude(ct => ct.Tag);
}
