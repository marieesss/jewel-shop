using JewelryShop.Features.Auth.Commands;
using JewelryShop.Features.Auth.Validators;

namespace JewelryShop.API.Tests.Auth;

public class RefreshSessionCommandValidatorTests
{
    private readonly RefreshSessionCommandValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("un-token")]
    public void Accepte_tout_token_meme_absent(string? token)
    {
        var result = _validator.Validate(new RefreshSessionCommand(token));

        Assert.True(result.IsValid);
    }
}
