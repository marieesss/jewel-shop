using JewelryShop.Features.Orders.Queries;
using JewelryShop.Features.Orders.Validators;

namespace JewelryShop.API.Tests.Orders;

public class GetQuoteQueryValidatorTests
{
    private readonly GetQuoteQueryValidator _validator = new();

    [Fact]
    public void Accepte_un_devis_valide()
    {
        var result = _validator.Validate(new GetQuoteQuery(1, 45, [5, 5]));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Applique_les_regles_de_composition_du_collier()
    {
        var result = _validator.Validate(new GetQuoteQuery(1, 45, Enumerable.Repeat(5L, 9).ToList()));

        ValidationAssert.HasError(result, nameof(GetQuoteQuery.CharmIds), "8 breloques maximum.");
    }
}
