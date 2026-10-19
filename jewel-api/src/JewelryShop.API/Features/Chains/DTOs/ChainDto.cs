namespace JewelryShop.Features.Chains.DTOs;

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
    int StockCm);

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
}
