using JewelryShop.Features.Invitations.Commands;
using JewelryShop.Features.Invitations.Validators;

namespace JewelryShop.API.Tests.Invitations;

public class CreateInvitationCommandValidatorTests
{
    private readonly CreateInvitationCommandValidator _validator = new();

    [Fact]
    public void Accepte_un_email_valide()
    {
        var result = _validator.Validate(new CreateInvitationCommand("nouvelle@exemple.com", 1));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "L'email est requis.")]
    [InlineData("nouvelle", "L'email n'est pas valide.")]
    public void Refuse_un_email_vide_ou_invalide(string email, string expectedMessage)
    {
        var result = _validator.Validate(new CreateInvitationCommand(email, 1));

        ValidationAssert.HasError(result, nameof(CreateInvitationCommand.Email), expectedMessage);
    }

    [Fact]
    public void Refuse_un_email_de_plus_de_255_caracteres()
    {
        var email = new string('a', 244) + "@exemple.com";

        var result = _validator.Validate(new CreateInvitationCommand(email, 1));

        ValidationAssert.HasError(result, nameof(CreateInvitationCommand.Email), "255 caractères maximum.");
    }
}
