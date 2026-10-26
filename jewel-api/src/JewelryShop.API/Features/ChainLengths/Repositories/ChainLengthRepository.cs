using JewelryShop.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Features.ChainLengths.Repositories;

public class ChainLengthRepository : Repository<ChainLength>, IChainLengthRepository
{
    public ChainLengthRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ChainLength>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().OrderBy(l => l.LengthCm).ToListAsync(cancellationToken);

    public Task<bool> ExistsByLengthAsync(int lengthCm, long? excludedId = null, CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(l => l.LengthCm == lengthCm && l.Id != excludedId, cancellationToken);
}
