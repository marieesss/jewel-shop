using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Validators;

namespace JewelryShop.API.Tests.Chains;

public class UploadChainImageCommandValidatorTests
{
    private readonly UploadChainImageCommandValidator _validator = new();

    private static UploadChainImageCommand ValidCommand() =>
        new(7, Stream.Null, 1024, "photo.jpg", "image/jpeg");

    [Fact]
    public void Accepte_un_fichier_non_vide()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_fichier_absent_ou_vide()
    {
        var result = _validator.Validate(ValidCommand() with { Length = 0 });

        ValidationAssert.HasError(result, "File", "Aucun fichier fourni.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Refuse_un_identifiant_negatif_ou_nul(long id)
    {
        var result = _validator.Validate(ValidCommand() with { ChainId = id });

        ValidationAssert.HasError(result, nameof(UploadChainImageCommand.ChainId), "Identifiant invalide.");
    }
}
