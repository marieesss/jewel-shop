using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Orders.Repositories;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>
/// Dépôt des commandes en mémoire. Les rouleaux et les stocks de breloques sont tenus ici, comme les colonnes
/// que modifient les UPDATE conditionnels ; une transaction qui échoue les remet dans leur état de départ.
/// </summary>
public class FakeOrderRepository : InMemoryRepository<Order>, IOrderRepository
{
    private long _nextNumber = 1;

    /// <summary>Longueur restante de chaque rouleau, par id de rouleau.</summary>
    public Dictionary<long, (long ChainId, int RemainingCm)> Rolls { get; } = new();

    /// <summary>Stock de chaque breloque, par id de breloque.</summary>
    public Dictionary<long, int> CharmStocks { get; } = new();

    public int TransactionCount { get; private set; }

    public Task<bool> ContainsChainAsync(long chainId, CancellationToken cancellationToken = default)
        => Task.FromResult(Lines.Any(l => l.ChainId == chainId));

    public Task<bool> ContainsCharmAsync(long charmId, CancellationToken cancellationToken = default)
        => Task.FromResult(Lines.Any(l => l.CharmId == charmId));

    public async Task InTransactionAsync(Func<Task> work, CancellationToken cancellationToken)
    {
        TransactionCount++;
        var rolls = new Dictionary<long, (long, int)>(Rolls);
        var stocks = new Dictionary<long, int>(CharmStocks);
        var orders = Entities.ToList();
        try
        {
            await work();
        }
        catch
        {
            Restore(Rolls, rolls);
            Restore(CharmStocks, stocks);
            Entities.Clear();
            Entities.AddRange(orders);
            throw;
        }
    }

    public Task<long?> TakeChainLengthAsync(long chainId, int lengthCm, CancellationToken cancellationToken)
    {
        var roll = Rolls
            .Where(r => r.Value.ChainId == chainId && r.Value.RemainingCm >= lengthCm)
            .OrderBy(r => r.Value.RemainingCm).ThenBy(r => r.Key)
            .Select(r => (long?)r.Key)
            .FirstOrDefault();

        if (roll is { } id)
            Rolls[id] = (chainId, Rolls[id].RemainingCm - lengthCm);

        return Task.FromResult(roll);
    }

    public Task<bool> TakeCharmStockAsync(long charmId, int quantity, CancellationToken cancellationToken)
    {
        if (CharmStocks.GetValueOrDefault(charmId) < quantity)
            return Task.FromResult(false);

        CharmStocks[charmId] -= quantity;
        return Task.FromResult(true);
    }

    public Task<long> NextNumberSequenceValueAsync(CancellationToken cancellationToken)
        => Task.FromResult(_nextNumber++);

    private IEnumerable<OrderLine> Lines => Entities.SelectMany(o => o.Lines);

    private static void Restore<TKey, TValue>(Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> snapshot)
        where TKey : notnull
    {
        target.Clear();
        foreach (var (key, value) in snapshot)
            target[key] = value;
    }
}
