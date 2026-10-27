using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Validators;

namespace JewelryShop.API.Tests.Chains;

public class UpdateChainCommandValidatorTests
{
    private readonly UpdateChainCommandValidator _validator = new();

    private static UpdateChainCommand ValidCommand() =>
        new(7, "Gourmette", "Maille fine", ProductColor.Gold, 12.50m, 39.90m, 45m, "https://img.mycharmly.eu/gourmette.jpg");

    private static UpdateChainCommand MeterCommand() =>
        new(7, "Gourmette", "Maille fine", ProductColor.Gold,
            CostPerMeter: 4m, PricePerMeter: 10m, ClaspCost: 1m, ClaspPrice: 3m);

    [Fact]
    public void Accepte_une_chaine_valide()
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
            Url = new string('a', 500),
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_identifiant_nul()
    {
        var result = _validator.Validate(ValidCommand() with { Id = 0 });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.Id), "Identifiant invalide.");
    }

    [Theory]
    [InlineData("", "Le nom est requis.")]
    [InlineData(256, "255 caractères maximum.")]
    public void Refuse_un_nom_invalide(object name, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Name = ValidationAssert.TextOf(name) });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.Name), expectedMessage);
    }

    [Fact]
    public void Refuse_une_description_de_plus_de_10000_caracteres()
    {
        var result = _validator.Validate(ValidCommand() with { Description = new string('a', 10001) });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.Description), "10000 caractères maximum.");
    }

    [Fact]
    public void Refuse_une_couleur_hors_enumeration()
    {
        var result = _validator.Validate(ValidCommand() with { Color = (ProductColor)99 });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.Color), "Couleur invalide.");
    }

    [Fact]
    public void Refuse_un_cout_negatif()
    {
        var result = _validator.Validate(ValidCommand() with { Cost = -0.01m });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.Cost), "Le coût doit être positif.");
    }

    [Fact]
    public void Refuse_un_prix_negatif()
    {
        var result = _validator.Validate(ValidCommand() with { Price = -0.01m });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.Price), "Le prix doit être positif.");
    }

    [Fact]
    public void Refuse_une_longueur_nulle()
    {
        var result = _validator.Validate(ValidCommand() with { Length = 0 });

        ValidationAssert.HasError(
            result,
            nameof(UpdateChainCommand.Length),
            "La longueur doit être supérieure à 0.");
    }

    [Fact]
    public void Refuse_une_url_de_plus_de_500_caracteres()
    {
        var result = _validator.Validate(ValidCommand() with { Url = new string('a', 501) });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.Url), "500 caractères maximum.");
    }

    [Fact]
    public void Accepte_le_nouveau_format_sans_les_anciens_champs()
    {
        var result = _validator.Validate(MeterCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Accepte_le_nouveau_format_sans_fermoir()
    {
        var result = _validator.Validate(MeterCommand() with { ClaspCost = null, ClaspPrice = null });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_prix_au_metre_negatif()
    {
        var result = _validator.Validate(MeterCommand() with { PricePerMeter = -0.01m });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.PricePerMeter), "Le prix doit être positif.");
    }

    [Fact]
    public void Refuse_un_cout_au_metre_negatif()
    {
        var result = _validator.Validate(MeterCommand() with { CostPerMeter = -0.01m });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.CostPerMeter), "Le coût doit être positif.");
    }

    [Fact]
    public void Refuse_un_fermoir_negatif()
    {
        var result = _validator.Validate(MeterCommand() with { ClaspCost = -0.01m, ClaspPrice = -0.01m });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.ClaspCost), "Le coût doit être positif.");
        ValidationAssert.HasError(result, nameof(UpdateChainCommand.ClaspPrice), "Le prix doit être positif.");
    }

    [Fact]
    public void Exige_le_prix_au_metre_quand_seul_le_cout_au_metre_est_fourni()
    {
        var result = _validator.Validate(MeterCommand() with { PricePerMeter = null });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.PricePerMeter), "Le prix au mètre est requis.");
    }

    [Fact]
    public void Exige_les_prix_au_metre_quand_l_ancien_format_est_incomplet()
    {
        var result = _validator.Validate(ValidCommand() with { Price = null });

        ValidationAssert.HasError(result, nameof(UpdateChainCommand.CostPerMeter), "Le coût au mètre est requis.");
        ValidationAssert.HasError(result, nameof(UpdateChainCommand.PricePerMeter), "Le prix au mètre est requis.");
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
