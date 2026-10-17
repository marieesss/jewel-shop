using JewelryShop.Features.Invitations.Queries;
using JewelryShop.Features.Invitations.Validators;

namespace JewelryShop.API.Tests.Invitations;

public class GetInvitationsQueryValidatorTests
{
    private readonly GetInvitationsQueryValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("Pending")]
    [InlineData("Expired")]
    [InlineData("Revoked")]
    [InlineData("Accepted")]
    [InlineData("pending")]
    public void Accepte_l_absence_de_statut_ou_un_statut_connu(string? status)
    {
        var result = _validator.Validate(new GetInvitationsQuery(status));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Inconnu")]
    [InlineData("1")]
    public void Refuse_un_statut_inconnu(string status)
    {
        var result = _validator.Validate(new GetInvitationsQuery(status));

        ValidationAssert.HasError(result, nameof(GetInvitationsQuery.Status), "Le statut n'est pas valide.");
    }
}
