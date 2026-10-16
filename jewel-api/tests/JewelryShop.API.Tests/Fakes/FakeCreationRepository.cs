using JewelryShop.Features.Creations.Entities;
using JewelryShop.Features.Creations.Repositories;

namespace JewelryShop.API.Tests.Fakes;

public class FakeCreationRepository : InMemoryRepository<Creation>, ICreationRepository
{
    public Task<Creation?> GetByIdWithItemsAsync(long id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<(IReadOnlyList<Creation> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
