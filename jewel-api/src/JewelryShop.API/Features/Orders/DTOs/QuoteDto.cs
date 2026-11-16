using JewelryShop.Features.Orders.Common;

namespace JewelryShop.Features.Orders.DTOs;

/// <summary>Ligne du devis : la chaîne porte sa longueur, chaque breloque sa position (à partir de 1).</summary>
public record QuoteLineDto(
    string Kind,
    long ProductId,
    string Name,
    int? LengthCm,
    int? Position,
    decimal UnitPrice);

public record QuoteDto(IReadOnlyList<QuoteLineDto> Lines, decimal Total);

public static class QuoteMappingExtensions
{
    public static QuoteDto ToQuoteDto(this Necklace necklace)
    {
        var chainLine = new QuoteLineDto(
            nameof(OrderLineKind.Chain), necklace.Chain.Id, necklace.Chain.Name, necklace.LengthCm, null, necklace.ChainPrice);

        var charmLines = necklace.Charms.Select((charm, index) => new QuoteLineDto(
            nameof(OrderLineKind.Charm), charm.Id, charm.Name, null, NecklaceRules.FirstPosition + index, charm.Price));

        return new QuoteDto([chainLine, .. charmLines], necklace.Total);
    }
}
