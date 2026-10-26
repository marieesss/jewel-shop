namespace JewelryShop.Features.Chains.DTOs;

/// <summary>
/// <see cref="AvailableLengths"/> et <see cref="FromPrice"/> ne sont calculés que par les endpoints
/// des chaînes : une chaîne incluse dans un favori ou une création les laisse à null.
/// </summary>
public record ChainDto(
    long Id,
    string Name,
    string Description,
    string Color,
    string? ImageUrl,
    string? Url,
    decimal? Cost,
    decimal? Price,
    decimal? Length,
    decimal? CostPerMeter,
    decimal? PricePerMeter,
    decimal ClaspCost,
    decimal ClaspPrice,
    int StockCm,
    IReadOnlyList<ChainLengthPriceDto>? AvailableLengths = null,
    decimal? FromPrice = null);

public record ChainLengthPriceDto(int LengthCm, string Label, decimal? Price, bool Available);

public static class ChainMappingExtensions
{
    public static ChainDto ToDto(this Chain c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.Color.ToString(),
        c.ImageUrl,
        c.Url,
        c.Cost,
        c.Price,
        c.Length,
        c.CostPerMeter,
        c.PricePerMeter,
        c.ClaspCost,
        c.ClaspPrice,
        c.StockCm);

    /// <summary>Ajoute au DTO le prix et la disponibilité de chaque longueur de référence.</summary>
    public static ChainDto ToDto(this Chain c, IEnumerable<ChainLength> lengths)
    {
        var lengthPrices = lengths
            .OrderBy(l => l.LengthCm)
            .Select(l => new ChainLengthPriceDto(l.LengthCm, l.Label, c.PriceFor(l.LengthCm), c.CanSell(l.LengthCm)))
            .ToList();

        return c.ToDto() with
        {
            AvailableLengths = lengthPrices,
            FromPrice = lengthPrices.Where(l => l.Available).Min(l => l.Price)
        };
    }
}
