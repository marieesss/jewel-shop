using JewelryShop.Features.Tags.Commands;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.Validators;

namespace JewelryShop.API.Tests.Tags;

public class TagCommandValidatorTests
{
    private readonly CreateTagCommandValidator _createValidator = new();
    private readonly RenameTagCommandValidator _renameValidator = new();
    private readonly MergeTagCommandValidator _mergeValidator = new();

    [Theory]
    [InlineData("Mer", "Theme")]
    [InlineData("Doré", "Color")]
    [InlineData("Argent 925", "Material")]
    [InlineData("Fête des mères", "Occasion")]
    public void Accepte_un_tag_valide(string name, string category)
    {
        var result = _createValidator.Validate(new CreateTagCommand(name, category));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Refuse_un_nom_vide_avec_un_seul_message(string? name)
    {
        var result = _createValidator.Validate(new CreateTagCommand(name!, "Theme"));

        ValidationAssert.HasError(result, nameof(CreateTagCommand.Name), TagMessages.NameRequired);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Accepte_un_nom_de_50_caracteres_entoure_d_espaces()
    {
        var result = _createValidator.Validate(new CreateTagCommand($"  {new string('a', 50)}  ", "Theme"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_nom_de_plus_de_50_caracteres()
    {
        var result = _createValidator.Validate(new CreateTagCommand(new string('a', 51), "Theme"));

        ValidationAssert.HasError(result, nameof(CreateTagCommand.Name), TagMessages.NameTooLong);
    }

    [Theory]
    [InlineData("!!!")]
    [InlineData("- * -")]
    public void Refuse_un_nom_sans_lettre_ni_chiffre(string name)
    {
        var result = _createValidator.Validate(new CreateTagCommand(name, "Theme"));

        ValidationAssert.HasError(result, nameof(CreateTagCommand.Name), TagMessages.NameWithoutLetterOrDigit);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("theme")]
    [InlineData("0")]
    [InlineData("Saison")]
    public void Refuse_une_categorie_inconnue(string? category)
    {
        var result = _createValidator.Validate(new CreateTagCommand("Mer", category!));

        ValidationAssert.HasError(result, nameof(CreateTagCommand.Category), TagMessages.InvalidCategory);
    }

    [Fact]
    public void Accepte_un_renommage_valide()
    {
        var result = _renameValidator.Validate(new RenameTagCommand(1, "Océan"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", TagMessages.NameRequired)]
    [InlineData(51, TagMessages.NameTooLong)]
    [InlineData("???", TagMessages.NameWithoutLetterOrDigit)]
    public void Applique_les_regles_du_nom_au_renommage(object name, string expectedMessage)
    {
        var result = _renameValidator.Validate(new RenameTagCommand(1, ValidationAssert.TextOf(name)));

        ValidationAssert.HasError(result, nameof(RenameTagCommand.Name), expectedMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Refuse_un_identifiant_invalide_au_renommage(long id)
    {
        var result = _renameValidator.Validate(new RenameTagCommand(id, "Océan"));

        ValidationAssert.HasError(result, nameof(RenameTagCommand.Id), TagMessages.InvalidId);
    }

    [Fact]
    public void Accepte_une_fusion_valide()
    {
        var result = _mergeValidator.Validate(new MergeTagCommand(1, 2));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_de_fusionner_un_tag_avec_lui_meme()
    {
        var result = _mergeValidator.Validate(new MergeTagCommand(3, 3));

        ValidationAssert.HasError(result, nameof(MergeTagCommand.TargetTagId), TagMessages.CannotMergeWithItself);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Refuse_un_identifiant_de_tag_source_invalide(long id)
    {
        var result = _mergeValidator.Validate(new MergeTagCommand(id, 2));

        ValidationAssert.HasError(result, nameof(MergeTagCommand.Id), TagMessages.InvalidId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Refuse_un_identifiant_de_tag_cible_invalide(long targetTagId)
    {
        var result = _mergeValidator.Validate(new MergeTagCommand(2, targetTagId));

        ValidationAssert.HasError(result, nameof(MergeTagCommand.TargetTagId), TagMessages.InvalidId);
    }
}
