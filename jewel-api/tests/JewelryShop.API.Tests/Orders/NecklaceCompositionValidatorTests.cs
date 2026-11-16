using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.Validators;

namespace JewelryShop.API.Tests.Orders;

public class NecklaceCompositionValidatorTests
{
    private sealed record Composition(long ChainId, int LengthCm, IReadOnlyList<long> CharmIds) : INecklaceComposition;

    private readonly NecklaceCompositionValidator<Composition> _validator = new();

    private static Composition WithCharms(int count) => new(1, 45, Enumerable.Repeat(5L, count).ToList());

    [Theory]
    [InlineData(1)]
    [InlineData(NecklaceRules.MaxCharms)]
    public void Accepte_un_collier_de_1_a_8_breloques(int count)
    {
        var result = _validator.Validate(WithCharms(count));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Refuse_un_collier_sans_breloque()
    {
        var result = _validator.Validate(WithCharms(0));

        ValidationAssert.HasError(result, nameof(Composition.CharmIds), "Ajoutez au moins une breloque.");
    }

    [Fact]
    public void Refuse_une_liste_de_breloques_absente()
    {
        var result = _validator.Validate(new Composition(1, 45, null!));

        ValidationAssert.HasError(result, nameof(Composition.CharmIds), "Ajoutez au moins une breloque.");
    }

    [Fact]
    public void Refuse_9_breloques()
    {
        var result = _validator.Validate(WithCharms(NecklaceRules.MaxCharms + 1));

        ValidationAssert.HasError(result, nameof(Composition.CharmIds), "8 breloques maximum.");
    }

    [Fact]
    public void Refuse_un_identifiant_de_chaine_nul()
    {
        var result = _validator.Validate(WithCharms(1) with { ChainId = 0 });

        ValidationAssert.HasError(result, nameof(Composition.ChainId), "Identifiant invalide.");
    }

    [Fact]
    public void Refuse_un_identifiant_de_breloque_nul()
    {
        var result = _validator.Validate(new Composition(1, 45, [5, 0]));

        ValidationAssert.HasError(result, "CharmIds[1]", "Identifiant invalide.");
    }
}
