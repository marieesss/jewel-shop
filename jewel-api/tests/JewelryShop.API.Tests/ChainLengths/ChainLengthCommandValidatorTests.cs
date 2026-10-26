using JewelryShop.Features.ChainLengths.Commands;
using JewelryShop.Features.ChainLengths.Common;
using JewelryShop.Features.ChainLengths.Validators;

namespace JewelryShop.API.Tests.ChainLengths;

public class ChainLengthCommandValidatorTests
{
    private readonly CreateChainLengthCommandValidator _createValidator = new();
    private readonly UpdateChainLengthCommandValidator _updateValidator = new();

    [Theory]
    [InlineData(10)]
    [InlineData(48)]
    [InlineData(200)]
    public void Accepte_une_longueur_entre_10_et_200_cm(int lengthCm)
    {
        var result = _createValidator.Validate(new CreateChainLengthCommand(lengthCm, "Princesse"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(201)]
    [InlineData(-5)]
    public void Refuse_une_longueur_hors_bornes(int lengthCm)
    {
        var result = _createValidator.Validate(new CreateChainLengthCommand(lengthCm, "Princesse"));

        ValidationAssert.HasError(result, nameof(CreateChainLengthCommand.LengthCm), ChainLengthMessages.LengthOutOfRange);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Refuse_un_libelle_vide(string label)
    {
        var result = _createValidator.Validate(new CreateChainLengthCommand(45, label));

        ValidationAssert.HasError(result, nameof(CreateChainLengthCommand.Label), ChainLengthMessages.LabelRequired);
    }

    [Fact]
    public void Accepte_un_libelle_de_50_caracteres()
    {
        var result = _createValidator.Validate(new CreateChainLengthCommand(45, new string('a', 50)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_libelle_de_plus_de_50_caracteres()
    {
        var result = _createValidator.Validate(new CreateChainLengthCommand(45, new string('a', 51)));

        ValidationAssert.HasError(result, nameof(CreateChainLengthCommand.Label), ChainLengthMessages.LabelTooLong);
    }

    [Fact]
    public void Applique_les_memes_regles_a_la_modification()
    {
        var result = _updateValidator.Validate(new UpdateChainLengthCommand(1, 201, ""));

        ValidationAssert.HasError(result, nameof(UpdateChainLengthCommand.LengthCm), ChainLengthMessages.LengthOutOfRange);
        ValidationAssert.HasError(result, nameof(UpdateChainLengthCommand.Label), ChainLengthMessages.LabelRequired);
    }

    [Fact]
    public void Accepte_une_modification_valide()
    {
        var result = _updateValidator.Validate(new UpdateChainLengthCommand(1, 45, "Princesse"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_identifiant_invalide_a_la_modification()
    {
        var result = _updateValidator.Validate(new UpdateChainLengthCommand(0, 45, "Princesse"));

        ValidationAssert.HasError(result, nameof(UpdateChainLengthCommand.Id), ChainLengthMessages.InvalidId);
    }
}
