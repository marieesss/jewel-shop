using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Orders.Repositories;

namespace JewelryShop.API.Tests.Fakes;

public class FakeOrderRepository : InMemoryRepository<Order>, IOrderRepository
{
    public Task<bool> ContainsChainAsync(long chainId, CancellationToken cancellationToken = default)
        => Task.FromResult(Lines.Any(l => l.ChainId == chainId));

    public Task<bool> ContainsCharmAsync(long charmId, CancellationToken cancellationToken = default)
        => Task.FromResult(Lines.Any(l => l.CharmId == charmId));

    private IEnumerable<OrderLine> Lines => Entities.SelectMany(o => o.Lines);
}
