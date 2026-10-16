using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Validators;

namespace JewelryShop.API.Tests.Auth;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Fact]
    public void Accepte_un_email_valide()
    {
        var result = _validator.Validate(new ForgotPasswordCommand("marie@mycharmly.eu"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "L'email est requis.")]
    [InlineData("marie", "L'email n'est pas valide.")]
    public void Refuse_un_email_invalide(string email, string expectedMessage)
    {
        var result = _validator.Validate(new ForgotPasswordCommand(email));

        ValidationAssert.HasError(result, nameof(ForgotPasswordCommand.Email), expectedMessage);
    }
}
