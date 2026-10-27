using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Features.Users.Queries;

namespace JewelryShop.API.Tests.Users;

public class GetMyProfileQueryHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly GetMyProfileQueryHandler _handler;

    public GetMyProfileQueryHandlerTests() => _handler = new GetMyProfileQueryHandler(_users);

    private static User NewUser(long id) => new()
    {
        Id = id,
        Name = "Marie",
        Surname = "Durand",
        Email = "marie@example.com",
        PasswordHash = "hash",
        Birthday = new DateOnly(1990, 5, 12)
    };

    [Fact]
    public async Task Renvoie_une_adresse_nulle_pour_un_utilisateur_sans_adresse()
    {
        _users.Seed(NewUser(7));

        var profile = await _handler.Handle(new GetMyProfileQuery(7), CancellationToken.None);

        Assert.Equal(7, profile.Id);
        Assert.Equal("Marie", profile.Name);
        Assert.Equal("Durand", profile.Surname);
        Assert.Equal("marie@example.com", profile.Email);
        Assert.Equal(new DateOnly(1990, 5, 12), profile.Birthday);
        Assert.Null(profile.Phone);
        Assert.Null(profile.Address);
    }

    [Fact]
    public async Task Renvoie_l_adresse_et_le_telephone_enregistres()
    {
        var user = NewUser(7);
        user.Phone = "0612345678";
        user.AddressLine1 = "12 rue des Lilas";
        user.AddressLine2 = "Bâtiment B";
        user.PostalCode = "75011";
        user.City = "Paris";
        _users.Seed(user);

        var profile = await _handler.Handle(new GetMyProfileQuery(7), CancellationToken.None);

        Assert.Equal("0612345678", profile.Phone);
        Assert.NotNull(profile.Address);
        Assert.Equal("12 rue des Lilas", profile.Address.Line1);
        Assert.Equal("Bâtiment B", profile.Address.Line2);
        Assert.Equal("75011", profile.Address.PostalCode);
        Assert.Equal("Paris", profile.Address.City);
    }

    [Fact]
    public async Task Leve_une_erreur_si_l_utilisateur_n_existe_plus()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetMyProfileQuery(42), CancellationToken.None));
    }
}
