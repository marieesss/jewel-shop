using JewelryShop.Application.Features.Auth.Commands;

namespace JewelryShop.API.Tests.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private static RegisterCommand ValidCommand() =>
        new("Marie", "Dupont", "marie@mycharmly.eu", "Secret123!", new DateOnly(1995, 4, 12));

    [Fact]
    public void Accepte_une_inscription_valide()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Le prénom est requis.")]
    [InlineData(101, "100 caractères maximum.")]
    public void Refuse_un_prenom_invalide(object name, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Name = ValidationAssert.TextOf(name) });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Name), expectedMessage);
    }

    [Theory]
    [InlineData("", "Le nom est requis.")]
    [InlineData(101, "100 caractères maximum.")]
    public void Refuse_un_nom_invalide(object surname, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Surname = ValidationAssert.TextOf(surname) });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Surname), expectedMessage);
    }

    [Theory]
    [InlineData("", "L'email est requis.")]
    [InlineData("pas-un-email", "L'email n'est pas valide.")]
    public void Refuse_un_email_invalide(string email, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Email = email });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Email), expectedMessage);
    }

    [Fact]
    public void Refuse_un_email_de_plus_de_255_caracteres()
    {
        var email = new string('a', 250) + "@x.com";

        var result = _validator.Validate(ValidCommand() with { Email = email });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Email), "255 caractères maximum.");
    }

    [Theory]
    [InlineData("", "Le mot de passe est requis.")]
    [InlineData("court", "Le mot de passe doit contenir au moins 8 caractères.")]
    [InlineData(101, "100 caractères maximum.")]
    public void Refuse_un_mot_de_passe_invalide(object password, string expectedMessage)
    {
        var result = _validator.Validate(ValidCommand() with { Password = ValidationAssert.TextOf(password) });

        ValidationAssert.HasError(result, nameof(RegisterCommand.Password), expectedMessage);
    }

    [Fact]
    public void Refuse_une_date_de_naissance_dans_le_futur()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(ValidCommand() with { Birthday = tomorrow });

        ValidationAssert.HasError(
            result,
            nameof(RegisterCommand.Birthday),
            "La date de naissance doit être dans le passé.");
    }
}
