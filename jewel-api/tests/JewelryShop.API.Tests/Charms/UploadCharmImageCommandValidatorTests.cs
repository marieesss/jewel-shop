using JewelryShop.Common.Validation;
using JewelryShop.Features.Charms.Commands;
using JewelryShop.Features.Charms.Validators;

namespace JewelryShop.API.Tests.Charms;

public class UploadCharmImageCommandValidatorTests
{
    private readonly UploadCharmImageCommandValidator _validator = new();

    private static UploadCharmImageCommand ValidCommand() =>
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

    [Fact]
    public void Ne_signale_que_l_absence_de_fichier_quand_aucun_fichier_n_est_envoye()
    {
        var result = _validator.Validate(ValidCommand() with { Length = 0, FileName = "", ContentType = "" });

        var error = Assert.Single(result.Errors);
        Assert.Equal("Aucun fichier fourni.", error.ErrorMessage);
    }

    [Theory]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.JPEG", "image/jpeg")]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.webp", "IMAGE/WEBP")]
    public void Accepte_une_image_jpeg_png_ou_webp(string fileName, string contentType)
    {
        var result = _validator.Validate(ValidCommand() with { FileName = fileName, ContentType = contentType });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("page.html", "text/html")]
    [InlineData("anim.gif", "image/gif")]
    [InlineData("setup.exe", "application/octet-stream")]
    public void Refuse_un_type_de_fichier_hors_liste_blanche(string fileName, string contentType)
    {
        var result = _validator.Validate(ValidCommand() with { FileName = fileName, ContentType = contentType });

        var error = Assert.Single(result.Errors);
        Assert.Equal(ImageFileRules.PropertyName, error.PropertyName);
        Assert.Equal(ImageFileRules.ContentTypeMessage, error.ErrorMessage);
    }

    [Theory]
    [InlineData("photo.png", "image/jpeg")]
    [InlineData("page.html", "image/png")]
    [InlineData("photo", "image/webp")]
    public void Refuse_une_extension_qui_ne_correspond_pas_au_type(string fileName, string contentType)
    {
        var result = _validator.Validate(ValidCommand() with { FileName = fileName, ContentType = contentType });

        ValidationAssert.HasError(result, ImageFileRules.PropertyName, ImageFileRules.ExtensionMessage);
    }

    [Fact]
    public void Accepte_une_image_de_exactement_la_taille_maximale()
    {
        var result = _validator.Validate(ValidCommand() with { Length = ImageFileRules.MaxSizeInBytes });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_une_image_plus_lourde_que_la_taille_maximale()
    {
        var result = _validator.Validate(ValidCommand() with { Length = ImageFileRules.MaxSizeInBytes + 1 });

        ValidationAssert.HasError(result, ImageFileRules.PropertyName, "L'image ne doit pas dépasser 5 Mo.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Refuse_un_identifiant_negatif_ou_nul(long id)
    {
        var result = _validator.Validate(ValidCommand() with { CharmId = id });

        ValidationAssert.HasError(result, nameof(UploadCharmImageCommand.CharmId), "Identifiant invalide.");
    }
}
