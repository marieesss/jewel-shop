using JewelryShop.Application.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Application.Features.Favorites;

public interface IFavoriteRepository : IRepository<Favorite>
{
    Task<(IReadOnlyList<Favorite> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        long userId, long? chainId, long? charmId, CancellationToken cancellationToken = default);
}

public class FavoriteRepository : Repository<Favorite>, IFavoriteRepository
{
    public FavoriteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(IReadOnlyList<Favorite> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(f => f.Chain)
            .Include(f => f.Charm)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> ExistsAsync(
        long userId, long? chainId, long? charmId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(
            f => f.UserId == userId && f.ChainId == chainId && f.CharmId == charmId,
            cancellationToken);
}
