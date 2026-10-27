using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.Commands;
using JewelryShop.Features.Charms.Validators;

namespace JewelryShop.API.Tests.Charms;

public class UpdateCharmCommandValidatorTests
{
    private readonly UpdateCharmCommandValidator _validator = new();

    private static UpdateCharmCommand ValidCommand() =>
        new(7, "Trèfle", "Breloque porte-bonheur", ProductColor.Silver, 3.20m, 14.90m, 12, "https://img.mycharmly.eu/trefle.jpg");

    [Fact]
    public void Accepte_une_breloque_valide()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Accepte_les_valeurs_limites()
    {
        var command = ValidCommand() with
        {
            Name = new string('a', 255),
            Description = new string('a', 10000),
            Cost = 0,
            Price = 0,
            Stock = 0,
            Url = new string('a', 500),
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_identifiant_nul()
    {
        var result = _validator.Validate(ValidCommand() with { Id = 0 });

        ValidationAssert.HasError(result, nameof(UpdateCharmCommand.Id), "Identifiant invalide.");
    }

    [Theory]
    [InlineData("", "Le nom est requis.")]
    [InlineData(256, "255 caractères maximum.")]
    public void Refuse_un_nom_invalide(object name, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Name = ValidationAssert.TextOf(name) });

        ValidationAssert.HasError(result, nameof(UpdateCharmCommand.Name), expectedMessage);
    }

    [Fact]
    public void Refuse_une_description_de_plus_de_10000_caracteres()
    {
        var result = _validator.Validate(ValidCommand() with { Description = new string('a', 10001) });

        ValidationAssert.HasError(result, nameof(UpdateCharmCommand.Description), "10000 caractères maximum.");
    }

    [Fact]
    public void Refuse_une_couleur_hors_enumeration()
    {
        var result = _validator.Validate(ValidCommand() with { Color = (ProductColor)99 });

        ValidationAssert.HasError(result, nameof(UpdateCharmCommand.Color), "Couleur invalide.");
    }

    [Fact]
    public void Refuse_un_cout_negatif()
    {
        var result = _validator.Validate(ValidCommand() with { Cost = -0.01m });

        ValidationAssert.HasError(result, nameof(UpdateCharmCommand.Cost), "Le coût doit être positif.");
    }

    [Fact]
    public void Refuse_un_prix_negatif()
    {
        var result = _validator.Validate(ValidCommand() with { Price = -0.01m });

        ValidationAssert.HasError(result, nameof(UpdateCharmCommand.Price), "Le prix doit être positif.");
    }

    [Fact]
    public void Refuse_un_stock_negatif()
    {
        var result = _validator.Validate(ValidCommand() with { Stock = -1 });

        ValidationAssert.HasError(result, nameof(UpdateCharmCommand.Stock), "Le stock doit être positif.");
    }

    [Fact]
    public void Refuse_une_url_de_plus_de_500_caracteres()
    {
        var result = _validator.Validate(ValidCommand() with { Url = new string('a', 501) });

        ValidationAssert.HasError(result, nameof(UpdateCharmCommand.Url), "500 caractères maximum.");
    }

    [Fact]
    public void Accepte_des_tags_sans_maximum_ou_une_liste_vide()
    {
        var many = Enumerable.Range(1, 500).Select(i => (long)i).ToArray();

        Assert.True(_validator.Validate(ValidCommand() with { TagIds = many }).IsValid);
        Assert.True(_validator.Validate(ValidCommand() with { TagIds = [] }).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Refuse_un_id_de_tag_invalide(long tagId)
    {
        var result = _validator.Validate(ValidCommand() with { TagIds = [1, tagId] });

        ValidationAssert.HasError(result, "TagIds[1]", "Identifiant invalide.");
    }
}
