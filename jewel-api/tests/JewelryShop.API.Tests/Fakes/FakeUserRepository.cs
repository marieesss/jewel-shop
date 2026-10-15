using JewelryShop.Application.Features.Users;

namespace JewelryShop.API.Tests.Fakes;

public class FakeUserRepository : InMemoryRepository<User>, IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.FirstOrDefault(u => u.Email == email));

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.Any(u => u.Email == email));

    public Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
        => Task.FromResult<(IReadOnlyList<User>, int)>((Entities.ToList(), Entities.Count));
}
