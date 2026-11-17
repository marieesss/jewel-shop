using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Orders.Repositories;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>
/// Dépôt des commandes en mémoire. Les rouleaux et les stocks de breloques sont tenus ici, comme les colonnes
/// que modifient les UPDATE conditionnels ; une transaction qui échoue les remet dans leur état de départ,
/// avec le statut et l'historique des commandes.
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

    public Task<Order?> GetByIdWithDetailsAsNoTrackingAsync(long id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<Order?> GetByIdWithDetailsAsync(long id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<Order?> GetByIdWithDetailsAndNotesAsNoTrackingAsync(long id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.Any(o => o.Id == id));

    public Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        OrderStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var matching = Entities
            .Where(o => status is null || o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToList();
        IReadOnlyList<Order> pageItems = matching.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((pageItems, matching.Count));
    }

    public Task<OrderNote?> GetNoteAsync(long orderId, long noteId, CancellationToken cancellationToken = default)
        => Task.FromResult(Notes.FirstOrDefault(n => n.Id == noteId && n.OrderId == orderId));

    /// <summary>Range la note dans sa commande, où la relecture de la commande avec ses notes la retrouve.</summary>
    public Task AddNoteAsync(OrderNote note, CancellationToken cancellationToken = default)
    {
        Entities.Single(o => o.Id == note.OrderId).Notes.Add(note);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var mine = Entities
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToList();
        IReadOnlyList<Order> pageItems = mine.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((pageItems, mine.Count));
    }

    public async Task InTransactionAsync(Func<Task> work, CancellationToken cancellationToken)
    {
        TransactionCount++;
        var rolls = new Dictionary<long, (long, int)>(Rolls);
        var stocks = new Dictionary<long, int>(CharmStocks);
        var orders = Entities.ToList();
        var states = orders.ToDictionary(o => o, o => (o.Status, o.UpdatedAt, o.StatusHistory.ToList()));
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
            foreach (var (order, (status, updatedAt, history)) in states)
            {
                order.Status = status;
                order.UpdatedAt = updatedAt;
                order.StatusHistory = history;
            }
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

    public Task<bool> TryChangeStatusAsync(
        long orderId, long userId, OrderStatus from, OrderStatus to, DateTime now, CancellationToken cancellationToken)
    {
        var order = Entities.SingleOrDefault(o => o.Id == orderId && o.UserId == userId && o.Status == from);
        if (order is null)
            return Task.FromResult(false);

        order.Status = to;
        order.UpdatedAt = now;
        return Task.FromResult(true);
    }

    public Task ReturnChainLengthAsync(long rollId, int lengthCm, CancellationToken cancellationToken)
    {
        if (Rolls.TryGetValue(rollId, out var roll))
            Rolls[rollId] = (roll.ChainId, roll.RemainingCm + lengthCm);
        return Task.CompletedTask;
    }

    public Task ReturnCharmStockAsync(long charmId, int quantity, CancellationToken cancellationToken)
    {
        if (CharmStocks.ContainsKey(charmId))
            CharmStocks[charmId] += quantity;
        return Task.CompletedTask;
    }

    /// <summary>Attribue un id aux nouvelles commandes et notes, comme les colonnes identity de la base.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var order in Entities.Where(o => o.Id == 0))
            order.Id = Entities.Max(o => o.Id) + 1;

        foreach (var note in Notes.Where(n => n.Id == 0).ToList())
            note.Id = Notes.Max(n => n.Id) + 1;

        return base.SaveChangesAsync(cancellationToken);
    }

    public Task<long> NextNumberSequenceValueAsync(CancellationToken cancellationToken)
        => Task.FromResult(_nextNumber++);

    private IEnumerable<OrderLine> Lines => Entities.SelectMany(o => o.Lines);

    private IEnumerable<OrderNote> Notes => Entities.SelectMany(o => o.Notes);

    private static void Restore<TKey, TValue>(Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> snapshot)
        where TKey : notnull
    {
        target.Clear();
        foreach (var (key, value) in snapshot)
            target[key] = value;
    }
}
