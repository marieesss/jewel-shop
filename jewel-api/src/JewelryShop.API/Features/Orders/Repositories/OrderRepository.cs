using JewelryShop.Common.Persistence;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Features.Orders.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public Task<bool> ContainsChainAsync(long chainId, CancellationToken cancellationToken = default)
        => Context.OrderLines.AnyAsync(l => l.ChainId == chainId, cancellationToken);

    public Task<bool> ContainsCharmAsync(long charmId, CancellationToken cancellationToken = default)
        => Context.OrderLines.AnyAsync(l => l.CharmId == charmId, cancellationToken);
}
