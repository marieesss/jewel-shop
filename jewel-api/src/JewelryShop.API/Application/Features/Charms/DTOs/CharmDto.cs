
namespace JewelryShop.Application.Features.Charms.DTOs;

public record CharmDto(
    long Id,
    string Name,
    string Description,
    string Color,
    string? Url,
    string? ImageUrl,
    decimal Cost,
    decimal Price,
    int Stock);

public static class CharmMappingExtensions
{
    public static CharmDto ToDto(this Charm c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.Color.ToString(),
        c.Url,
        c.ImageUrl,
        c.Cost,
        c.Price,
        c.Stock);
}
