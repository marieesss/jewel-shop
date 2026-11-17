using JewelryShop.Common.Enums;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Features.Users.Repositories;

namespace JewelryShop.API.Tests.Fakes;

public class FakeUserRepository : InMemoryRepository<User>, IUserRepository
{
    public Task<User?> GetByIdAsNoTrackingAsync(long id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<User>> GetByIdsAsNoTrackingAsync(
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<User>>(Entities.Where(u => ids.Contains(u.Id)).ToList());

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.FirstOrDefault(u => u.Email == email));

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.Any(u => u.Email == email));

    public Task<IReadOnlyList<User>> GetByRoleAsNoTrackingAsync(
        UserRole role, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<User>>(Entities.Where(u => u.Role == role).OrderBy(u => u.Id).ToList());

    public Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
        => Task.FromResult<(IReadOnlyList<User>, int)>((Entities.ToList(), Entities.Count));
}
