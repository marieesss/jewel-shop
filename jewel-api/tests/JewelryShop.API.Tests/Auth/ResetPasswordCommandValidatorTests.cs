using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Common;
using JewelryShop.Features.Auth.Validators;

namespace JewelryShop.API.Tests.Auth;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    [Fact]
    public void Accepte_un_token_et_un_mot_de_passe_valides()
    {
        var result = _validator.Validate(new ResetPasswordCommand("token", "Secret123!"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_token_vide_avec_le_message_du_lien_invalide()
    {
        var result = _validator.Validate(new ResetPasswordCommand("", "Secret123!"));

        ValidationAssert.HasError(result, nameof(ResetPasswordCommand.Token), PasswordResetTokenDefaults.InvalidTokenMessage);
    }

    [Theory]
    [InlineData("", "Le mot de passe est requis.")]
    [InlineData("court", "Le mot de passe doit contenir au moins 8 caractères.")]
    [InlineData("secret123!", "Le mot de passe doit contenir au moins une majuscule.")]
    public void Applique_la_politique_de_mot_de_passe(string password, string expectedMessage)
    {
        var result = _validator.Validate(new ResetPasswordCommand("token", password));

        ValidationAssert.HasError(result, nameof(ResetPasswordCommand.Password), expectedMessage);
    }
}
