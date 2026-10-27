using JewelryShop.Features.ChainLengths.Entities;
using JewelryShop.Features.ChainLengths.Repositories;

namespace JewelryShop.API.Tests.Fakes;

public class FakeChainLengthRepository : InMemoryRepository<ChainLength>, IChainLengthRepository
{
    public Task<IReadOnlyList<ChainLength>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ChainLength>>(Entities.OrderBy(l => l.LengthCm).ToList());

    public Task<bool> ExistsByLengthAsync(int lengthCm, long? excludedId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.Any(l => l.LengthCm == lengthCm && l.Id != excludedId));
}
