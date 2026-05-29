using JewelryShop.Application.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Application.Features.Creations;

public interface ICreationRepository : IRepository<Creation>
{
    Task<Creation?> GetByIdWithItemsAsync(long id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Creation> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default);
}

public class CreationRepository : Repository<Creation>, ICreationRepository
{
    public CreationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Creation?> GetByIdWithItemsAsync(long id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(c => c.Chain)
            .Include(c => c.Option)
            .Include(c => c.Items.OrderBy(i => i.Position))
                .ThenInclude(i => i.Charm)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Creation> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(c => c.Chain)
            .Include(c => c.Option)
            .Include(c => c.Items.OrderBy(i => i.Position))
                .ThenInclude(i => i.Charm)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
