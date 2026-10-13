using JewelryShop.Application.Common;
using JewelryShop.Application.Common.Persistence;

namespace JewelryShop.API.Tests.Fakes;

public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly List<T> Entities = new();

    public int SaveChangesCount { get; private set; }

    public IReadOnlyList<T> All => Entities;

    public void Seed(params T[] entities) => Entities.AddRange(entities);

    public Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.FirstOrDefault(e => e.Id == id));

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<T>>(Entities.ToList());

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Entities.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(T entity)
    {
    }

    public void Delete(T entity) => Entities.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}
