using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Validators;
using JewelryShop.Features.Users.Common;

namespace JewelryShop.API.Tests.Orders;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    private static readonly OrderRecipientInput _recipient = new("Alice", "Martin", "0612345678");
    private static readonly OrderAddressInput _address = new("12 rue des Lilas", null, "75011", "Paris");

    private static PlaceOrderCommand Valid() =>
        new(UserId: 4, ChainId: 3, LengthCm: 45, CharmIds: [10, 10, 11], Comment: null,
            Recipient: null, Address: null, SaveAddressToProfile: false);

    [Fact]
    public void Accepte_une_commande_sans_destinataire_ni_adresse()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Accepte_un_destinataire_et_une_adresse_complets()
    {
        var result = _validator.Validate(Valid() with { Recipient = _recipient, Address = _address, Comment = "Pour Noël" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Accepte_un_destinataire_sans_telephone()
    {
        var result = _validator.Validate(Valid() with { Recipient = _recipient with { Phone = null } });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Reprend_les_regles_du_collier_du_devis()
    {
        var result = _validator.Validate(Valid() with { CharmIds = [] });

        ValidationAssert.HasError(result, nameof(PlaceOrderCommand.CharmIds), "Ajoutez au moins une breloque.");
    }

    [Fact]
    public void Refuse_un_utilisateur_invalide()
    {
        var result = _validator.Validate(Valid() with { UserId = 0 });

        ValidationAssert.HasError(result, nameof(PlaceOrderCommand.UserId), "Identifiant invalide.");
    }

    [Fact]
    public void Accepte_un_commentaire_de_1000_caracteres()
    {
        var result = _validator.Validate(Valid() with { Comment = new string('a', 1000) });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_commentaire_de_1001_caracteres()
    {
        var result = _validator.Validate(Valid() with { Comment = new string('a', 1001) });

        ValidationAssert.HasError(result, nameof(PlaceOrderCommand.Comment), "1000 caractères maximum.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Refuse_un_destinataire_sans_prenom(string name)
    {
        var result = _validator.Validate(Valid() with { Recipient = _recipient with { Name = name } });

        ValidationAssert.HasError(result, "Recipient.Name", "Le prénom est requis.");
    }

    [Fact]
    public void Refuse_un_destinataire_sans_nom()
    {
        var result = _validator.Validate(Valid() with { Recipient = _recipient with { Surname = "" } });

        ValidationAssert.HasError(result, "Recipient.Surname", "Le nom est requis.");
    }

    [Fact]
    public void Refuse_un_prenom_et_un_nom_de_101_caracteres()
    {
        var tooLong = new string('a', 101);

        var result = _validator.Validate(Valid() with { Recipient = _recipient with { Name = tooLong, Surname = tooLong } });

        ValidationAssert.HasError(result, "Recipient.Name", "100 caractères maximum.");
        ValidationAssert.HasError(result, "Recipient.Surname", "100 caractères maximum.");
    }

    [Theory]
    [InlineData("0612345678")]
    [InlineData("+33612345678")]
    public void Accepte_un_telephone_francais(string phone)
    {
        var result = _validator.Validate(Valid() with { Recipient = _recipient with { Phone = phone } });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("612345678")]
    [InlineData("06 12 34 56 78")]
    [InlineData("0012345678")]
    public void Refuse_un_telephone_mal_forme(string phone)
    {
        var result = _validator.Validate(Valid() with { Recipient = _recipient with { Phone = phone } });

        ValidationAssert.HasError(result, "Recipient.Phone", UserMessages.PhoneInvalid);
    }

    [Fact]
    public void Refuse_une_adresse_sans_ligne_ni_ville()
    {
        var result = _validator.Validate(Valid() with { Address = _address with { Line1 = "", City = "" } });

        ValidationAssert.HasError(result, "Address.Line1", "L'adresse est requise.");
        ValidationAssert.HasError(result, "Address.City", "La ville est requise.");
    }

    [Theory]
    [InlineData("7501")]
    [InlineData("750011")]
    [InlineData("75O11")]
    [InlineData("")]
    public void Refuse_un_code_postal_qui_n_a_pas_5_chiffres(string postalCode)
    {
        var result = _validator.Validate(Valid() with { Address = _address with { PostalCode = postalCode } });

        ValidationAssert.HasError(result, "Address.PostalCode", "Le code postal doit contenir 5 chiffres.");
    }

    [Fact]
    public void Refuse_des_lignes_d_adresse_de_256_caracteres_et_une_ville_de_101()
    {
        var line = new string('a', 256);

        var result = _validator.Validate(Valid() with
        {
            Address = _address with { Line1 = line, Line2 = line, City = new string('a', 101) }
        });

        ValidationAssert.HasError(result, "Address.Line1", "255 caractères maximum.");
        ValidationAssert.HasError(result, "Address.Line2", "255 caractères maximum.");
        ValidationAssert.HasError(result, "Address.City", "100 caractères maximum.");
    }
}
