using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Users.Commands;
using JewelryShop.Features.Users.Entities;

namespace JewelryShop.API.Tests.Users;

public class UpdateMyAddressCommandHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly UpdateMyAddressCommandHandler _handler;
    private readonly User _marie = new()
    {
        Id = 7,
        Name = "Marie",
        Surname = "Durand",
        Email = "marie@example.com",
        PasswordHash = "hash"
    };

    public UpdateMyAddressCommandHandlerTests()
    {
        _users.Seed(_marie);
        _handler = new UpdateMyAddressCommandHandler(_users);
    }

    [Fact]
    public async Task Enregistre_l_adresse_nettoyee_et_le_telephone()
    {
        var command = new UpdateMyAddressCommand(7, "  12 rue des Lilas ", " Bâtiment B ", "75011", " Paris ", "+33612345678");

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("12 rue des Lilas", _marie.AddressLine1);
        Assert.Equal("Bâtiment B", _marie.AddressLine2);
        Assert.Equal("75011", _marie.PostalCode);
        Assert.Equal("Paris", _marie.City);
        Assert.Equal("+33612345678", _marie.Phone);
        Assert.Equal(1, _users.SaveChangesCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Enregistre_un_complement_vide_comme_absent(string? line2)
    {
        _marie.AddressLine2 = "Ancien complément";

        await _handler.Handle(
            new UpdateMyAddressCommand(7, "12 rue des Lilas", line2, "75011", "Paris", "0612345678"), CancellationToken.None);

        Assert.Null(_marie.AddressLine2);
    }

    [Fact]
    public async Task Renvoie_le_profil_avec_la_nouvelle_adresse()
    {
        var profile = await _handler.Handle(
            new UpdateMyAddressCommand(7, "12 rue des Lilas", null, "75011", "Paris", "0612345678"), CancellationToken.None);

        Assert.Equal(7, profile.Id);
        Assert.Equal("marie@example.com", profile.Email);
        Assert.Equal("0612345678", profile.Phone);
        Assert.NotNull(profile.Address);
        Assert.Equal("12 rue des Lilas", profile.Address.Line1);
        Assert.Null(profile.Address.Line2);
        Assert.Equal("75011", profile.Address.PostalCode);
        Assert.Equal("Paris", profile.Address.City);
    }

    [Fact]
    public async Task Leve_une_erreur_si_l_utilisateur_n_existe_plus()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(
            new UpdateMyAddressCommand(42, "12 rue des Lilas", null, "75011", "Paris", "0612345678"), CancellationToken.None));

        Assert.Equal(0, _users.SaveChangesCount);
    }
}
