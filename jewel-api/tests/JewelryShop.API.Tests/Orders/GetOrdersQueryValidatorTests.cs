using JewelryShop.Features.Orders.Queries;
using JewelryShop.Features.Orders.Validators;

namespace JewelryShop.API.Tests.Orders;

public class GetOrdersQueryValidatorTests
{
    private readonly GetOrdersQueryValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Pending")]
    [InlineData("Shipped")]
    [InlineData("cancelled")]
    public void Accepte_l_absence_de_statut_ou_un_statut_connu(string? status)
    {
        var result = _validator.Validate(new GetOrdersQuery(status));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Inconnu")]
    [InlineData("1")]
    public void Refuse_un_statut_inconnu(string status)
    {
        var result = _validator.Validate(new GetOrdersQuery(status));

        ValidationAssert.HasError(result, nameof(GetOrdersQuery.Status), "Le statut n'est pas valide.");
    }
}
