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

    public Task<Order?> GetByIdWithDetailsAsNoTrackingAsync(long id, CancellationToken cancellationToken = default)
        => DbSet.AsNoTracking()
            .Include(o => o.Lines)
            .Include(o => o.StatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task InTransactionAsync(Func<Task> work, CancellationToken cancellationToken)
    {
        // EnableRetryOnFailure interdit une transaction ouverte à la main hors de la stratégie
        // d'exécution. Si la stratégie rejoue le bloc, on repart d'un contexte vide.
        var strategy = Context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            Context.ChangeTracker.Clear();
            await using var transaction = await Context.Database.BeginTransactionAsync(cancellationToken);
            await work();
            await transaction.CommitAsync(cancellationToken);
        });
    }

    public async Task<long?> TakeChainLengthAsync(long chainId, int lengthCm, CancellationToken cancellationToken)
    {
        var candidateRollIds = await Context.Database
            .SqlQuery<long>($"""
                SELECT id AS "Value" FROM chain_rolls
                WHERE chain_id = {chainId} AND remaining_length_cm >= {lengthCm}
                ORDER BY remaining_length_cm, id
                """)
            .ToListAsync(cancellationToken);

        // Une commande simultanée peut avoir entamé un candidat depuis la lecture : la condition
        // de l'UPDATE le refuse alors, et on passe au rouleau suivant.
        foreach (var rollId in candidateRollIds)
        {
            var updated = await Context.Database.ExecuteSqlAsync($"""
                UPDATE chain_rolls SET remaining_length_cm = remaining_length_cm - {lengthCm}
                WHERE id = {rollId} AND remaining_length_cm >= {lengthCm}
                """, cancellationToken);

            if (updated == 1)
                return rollId;
        }

        return null;
    }

    public async Task<bool> TakeCharmStockAsync(long charmId, int quantity, CancellationToken cancellationToken)
        => await Context.Database.ExecuteSqlAsync($"""
            UPDATE charms SET stock = stock - {quantity}
            WHERE id = {charmId} AND stock >= {quantity}
            """, cancellationToken) == 1;

    public Task<long> NextNumberSequenceValueAsync(CancellationToken cancellationToken)
        => Context.Database
            .SqlQuery<long>($"""SELECT nextval('order_number_seq') AS "Value" """)
            .SingleAsync(cancellationToken);
}
