
namespace JewelryShop.Application.Features.Chains.DTOs;

public record ChainDto(
    long Id,
    string Name,
    string Description,
    string Color,
    string? Url,
    string? ImageUrl,
    decimal Cost,
    decimal Price,
    decimal Length);

public static class ChainMappingExtensions
{
    public static ChainDto ToDto(this Chain c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.Color.ToString(),
        c.Url,
        c.ImageUrl,
        c.Cost,
        c.Price,
        c.Length);
}
