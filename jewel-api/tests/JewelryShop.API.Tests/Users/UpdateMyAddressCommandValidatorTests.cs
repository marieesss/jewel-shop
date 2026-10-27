using JewelryShop.Features.Users.Commands;
using JewelryShop.Features.Users.Common;
using JewelryShop.Features.Users.Validators;

namespace JewelryShop.API.Tests.Users;

public class UpdateMyAddressCommandValidatorTests
{
    private readonly UpdateMyAddressCommandValidator _validator = new();

    private static UpdateMyAddressCommand Valid() =>
        new(UserId: 4, Line1: "12 rue des Lilas", Line2: null, PostalCode: "75011", City: "Paris", Phone: "0612345678");

    [Fact]
    public void Accepte_une_adresse_complete_valide()
    {
        var result = _validator.Validate(Valid() with { Line2 = "Bâtiment B" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Accepte_une_adresse_sans_complement()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_utilisateur_invalide()
    {
        var result = _validator.Validate(Valid() with { UserId = 0 });

        ValidationAssert.HasError(result, nameof(UpdateMyAddressCommand.UserId), UserMessages.InvalidId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Refuse_une_adresse_vide(string? line1)
    {
        var result = _validator.Validate(Valid() with { Line1 = line1! });

        ValidationAssert.HasError(result, nameof(UpdateMyAddressCommand.Line1), UserMessages.AddressLine1Required);
    }

    [Fact]
    public void Accepte_une_adresse_et_un_complement_de_255_caracteres()
    {
        var result = _validator.Validate(Valid() with { Line1 = new string('a', 255), Line2 = new string('b', 255) });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_une_adresse_de_plus_de_255_caracteres()
    {
        var result = _validator.Validate(Valid() with { Line1 = new string('a', 256) });

        ValidationAssert.HasError(result, nameof(UpdateMyAddressCommand.Line1), UserMessages.AddressLineTooLong);
    }

    [Fact]
    public void Refuse_un_complement_d_adresse_de_plus_de_255_caracteres()
    {
        var result = _validator.Validate(Valid() with { Line2 = new string('a', 256) });

        ValidationAssert.HasError(result, nameof(UpdateMyAddressCommand.Line2), UserMessages.AddressLineTooLong);
    }

    [Theory]
    [InlineData("7500")]
    [InlineData("750011")]
    [InlineData("75 011")]
    [InlineData("2A004")]
    [InlineData("75011\n")]
    [InlineData("٧٥٠١١")]
    [InlineData("")]
    [InlineData(null)]
    public void Refuse_un_code_postal_qui_n_a_pas_5_chiffres(string? postalCode)
    {
        var result = _validator.Validate(Valid() with { PostalCode = postalCode! });

        ValidationAssert.HasError(result, nameof(UpdateMyAddressCommand.PostalCode), UserMessages.PostalCodeInvalid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Refuse_une_ville_vide(string? city)
    {
        var result = _validator.Validate(Valid() with { City = city! });

        ValidationAssert.HasError(result, nameof(UpdateMyAddressCommand.City), UserMessages.CityRequired);
    }

    [Fact]
    public void Accepte_une_ville_de_100_caracteres()
    {
        var result = _validator.Validate(Valid() with { City = new string('a', 100) });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_une_ville_de_plus_de_100_caracteres()
    {
        var result = _validator.Validate(Valid() with { City = new string('a', 101) });

        ValidationAssert.HasError(result, nameof(UpdateMyAddressCommand.City), UserMessages.CityTooLong);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Refuse_un_telephone_vide(string? phone)
    {
        var result = _validator.Validate(Valid() with { Phone = phone! });

        ValidationAssert.HasError(result, nameof(UpdateMyAddressCommand.Phone), UserMessages.PhoneRequired);
    }

    [Theory]
    [InlineData("0612345678")]
    [InlineData("0145678901")]
    [InlineData("+33612345678")]
    public void Accepte_un_telephone_francais(string phone)
    {
        var result = _validator.Validate(Valid() with { Phone = phone });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("061234567")]
    [InlineData("06123456789")]
    [InlineData("0012345678")]
    [InlineData("0612345678\n")]
    [InlineData("06 12 34 56 78")]
    [InlineData("+330612345678")]
    [InlineData("+3261234567")]
    [InlineData("612345678")]
    public void Refuse_un_telephone_mal_forme(string phone)
    {
        var result = _validator.Validate(Valid() with { Phone = phone });

        ValidationAssert.HasError(result, nameof(UpdateMyAddressCommand.Phone), UserMessages.PhoneInvalid);
    }
}
