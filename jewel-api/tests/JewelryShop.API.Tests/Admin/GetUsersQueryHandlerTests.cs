using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Admin.Queries;
using JewelryShop.Features.Users.Entities;

namespace JewelryShop.API.Tests.Admin;

public class GetUsersQueryHandlerTests
{
    private static readonly DateTime _registeredAt = new(2026, 10, 15, 8, 30, 0, DateTimeKind.Utc);

    private readonly FakeUserRepository _users = new();
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _handler = new GetUsersQueryHandler(_users);
        _users.Seed(
            new User { Id = 1, Name = "Ancienne", Surname = "Cliente", Email = "ancienne@exemple.com" },
            new User { Id = 2, Name = "Nouvelle", Surname = "Cliente", Email = "nouvelle@exemple.com", CreatedAt = _registeredAt });
    }

    [Fact]
    public async Task Renvoie_la_date_d_inscription_d_un_compte_cree_apres_la_migration()
    {
        var result = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

        var user = Assert.Single(result.Items, u => u.Id == 2);
        Assert.Equal(_registeredAt, user.CreatedAt);
    }

    [Fact]
    public async Task Renvoie_une_date_d_inscription_nulle_pour_un_compte_existant_avant_la_migration()
    {
        var result = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

        var user = Assert.Single(result.Items, u => u.Id == 1);
        Assert.Null(user.CreatedAt);
    }
}
