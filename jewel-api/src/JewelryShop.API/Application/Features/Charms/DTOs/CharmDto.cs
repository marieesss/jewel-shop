namespace JewelryShop.Application.Features.Charms.DTOs;

public record CharmDto(
    long Id,
    string Name,
    string Description,
    string Color,
    string? ImageUrl,
    string? Url,
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
        c.ImageUrl,
        c.Url,
        c.Cost,
        c.Price,
        c.Stock);
}
