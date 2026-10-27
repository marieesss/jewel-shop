using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Users.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdAsNoTrackingAsync(long id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default);
}
