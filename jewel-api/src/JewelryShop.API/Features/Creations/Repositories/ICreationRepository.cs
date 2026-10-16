using JewelryShop.Application.Common.Persistence;

namespace JewelryShop.Application.Features.Creations;

public interface ICreationRepository : IRepository<Creation>
{
    Task<Creation?> GetByIdWithItemsAsync(long id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Creation> Items, int TotalCount)> GetPagedByUserAsync(
        long userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
