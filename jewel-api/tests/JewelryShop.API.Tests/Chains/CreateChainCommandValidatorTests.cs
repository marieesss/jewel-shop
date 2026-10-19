using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Validators;

namespace JewelryShop.API.Tests.Chains;

public class CreateChainCommandValidatorTests
{
    private readonly CreateChainCommandValidator _validator = new();

    private static CreateChainCommand ValidCommand() =>
        new("Gourmette", "Maille fine", ProductColor.Gold, 12.50m, 39.90m, 45m, "https://img.mycharmly.eu/gourmette.jpg");

    private static CreateChainCommand MeterCommand() =>
        new("Gourmette", "Maille fine", ProductColor.Gold,
            CostPerMeter: 4m, PricePerMeter: 10m, ClaspCost: 1m, ClaspPrice: 3m, InitialRollLengthCm: 500);

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

    [Theory]
    [InlineData("", "Le nom est requis.")]
    [InlineData(256, "255 caractères maximum.")]
    public void Refuse_un_nom_invalide(object name, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Name = ValidationAssert.TextOf(name) });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.Name), expectedMessage);
    }

    [Fact]
    public void Refuse_une_description_de_plus_de_10000_caracteres()
    {
        var result = _validator.Validate(ValidCommand() with { Description = new string('a', 10001) });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.Description), "10000 caractères maximum.");
    }

    [Fact]
    public void Refuse_une_couleur_hors_enumeration()
    {
        var result = _validator.Validate(ValidCommand() with { Color = (ProductColor)99 });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.Color), "Couleur invalide.");
    }

    [Fact]
    public void Refuse_un_cout_negatif()
    {
        var result = _validator.Validate(ValidCommand() with { Cost = -0.01m });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.Cost), "Le coût doit être positif.");
    }

    [Fact]
    public void Refuse_un_prix_negatif()
    {
        var result = _validator.Validate(ValidCommand() with { Price = -0.01m });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.Price), "Le prix doit être positif.");
    }

    [Fact]
    public void Refuse_une_longueur_nulle()
    {
        var result = _validator.Validate(ValidCommand() with { Length = 0 });

        ValidationAssert.HasError(
            result,
            nameof(CreateChainCommand.Length),
            "La longueur doit être supérieure à 0.");
    }

    [Fact]
    public void Refuse_une_url_de_plus_de_500_caracteres()
    {
        var result = _validator.Validate(ValidCommand() with { Url = new string('a', 501) });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.Url), "500 caractères maximum.");
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

        ValidationAssert.HasError(result, nameof(CreateChainCommand.PricePerMeter), "Le prix doit être positif.");
    }

    [Fact]
    public void Refuse_un_cout_au_metre_negatif()
    {
        var result = _validator.Validate(MeterCommand() with { CostPerMeter = -0.01m });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.CostPerMeter), "Le coût doit être positif.");
    }

    [Fact]
    public void Refuse_un_fermoir_negatif()
    {
        var result = _validator.Validate(MeterCommand() with { ClaspCost = -0.01m, ClaspPrice = -0.01m });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.ClaspCost), "Le coût doit être positif.");
        ValidationAssert.HasError(result, nameof(CreateChainCommand.ClaspPrice), "Le prix doit être positif.");
    }

    [Fact]
    public void Exige_le_cout_au_metre_quand_seul_le_prix_au_metre_est_fourni()
    {
        var result = _validator.Validate(MeterCommand() with { CostPerMeter = null });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.CostPerMeter), "Le coût au mètre est requis.");
    }

    [Fact]
    public void Exige_les_prix_au_metre_quand_l_ancien_format_est_incomplet()
    {
        var result = _validator.Validate(ValidCommand() with { Length = null });

        ValidationAssert.HasError(result, nameof(CreateChainCommand.CostPerMeter), "Le coût au mètre est requis.");
        ValidationAssert.HasError(result, nameof(CreateChainCommand.PricePerMeter), "Le prix au mètre est requis.");
    }

    [Fact]
    public void Exige_la_longueur_du_premier_rouleau_au_nouveau_format()
    {
        var result = _validator.Validate(MeterCommand() with { InitialRollLengthCm = null });

        ValidationAssert.HasError(
            result,
            nameof(CreateChainCommand.InitialRollLengthCm),
            "La longueur du rouleau doit être supérieure à 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Refuse_une_longueur_de_rouleau_nulle_ou_negative(int lengthCm)
    {
        var result = _validator.Validate(MeterCommand() with { InitialRollLengthCm = lengthCm });

        ValidationAssert.HasError(
            result,
            nameof(CreateChainCommand.InitialRollLengthCm),
            "La longueur du rouleau doit être supérieure à 0.");
    }

    [Fact]
    public void N_exige_pas_de_longueur_de_rouleau_a_l_ancien_format()
    {
        var result = _validator.Validate(ValidCommand() with { InitialRollLengthCm = null });

        Assert.True(result.IsValid);
    }
}
