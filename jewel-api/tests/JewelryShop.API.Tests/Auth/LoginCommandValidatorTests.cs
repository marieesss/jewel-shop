using JewelryShop.Features.Auth.Commands;

namespace JewelryShop.API.Tests.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    private static LoginCommand ValidCommand() => new("marie@mycharmly.eu", "Secret123!");

    [Fact]
    public void Accepte_une_connexion_valide()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Accepte_un_ancien_mot_de_passe_qui_ne_respecte_pas_la_politique()
    {
        var result = _validator.Validate(ValidCommand() with { Password = "faible" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "L'email est requis.")]
    [InlineData("pas-un-email", "L'email n'est pas valide.")]
    public void Refuse_un_email_invalide(string email, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Email = email });

        ValidationAssert.HasError(result, nameof(LoginCommand.Email), expectedMessage);
    }

    [Fact]
    public void Refuse_un_mot_de_passe_vide()
    {
        var result = _validator.Validate(ValidCommand() with { Password = "" });

        ValidationAssert.HasError(result, nameof(LoginCommand.Password), "Le mot de passe est requis.");
    }
}
