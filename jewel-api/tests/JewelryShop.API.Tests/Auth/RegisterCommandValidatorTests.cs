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
    [InlineData("")]
    [InlineData("pas-un-email")]
    public void Refuse_un_email_invalide(string email)
    {
        var result = _validator.Validate(ValidCommand() with { Email = email });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Fact]
    public void Refuse_un_mot_de_passe_de_moins_de_8_caracteres()
    {
        var result = _validator.Validate(ValidCommand() with { Password = "court" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Refuse_une_date_de_naissance_dans_le_futur()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(ValidCommand() with { Birthday = tomorrow });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.Birthday));
    }
}
