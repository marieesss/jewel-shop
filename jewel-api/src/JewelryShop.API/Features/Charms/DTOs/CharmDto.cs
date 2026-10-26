using System.Text.Json.Serialization;

namespace JewelryShop.Features.Charms.DTOs;

/// <summary>Breloque vue par un client : sans lien fournisseur ni coût d'achat.</summary>
// Une AdminCharmDto renvoyée là où une CharmDto est déclarée (liste paginée, query) garde ses
// champs admin à la sérialisation, au lieu d'être tronquée au type déclaré.
[JsonDerivedType(typeof(AdminCharmDto))]
public record CharmDto(
    long Id,
    string Name,
    string Description,
    string Color,
    string? ImageUrl,
    decimal Price,
    int Stock);

/// <summary>Breloque vue par un admin : ajoute le lien fournisseur et le coût d'achat.</summary>
public record AdminCharmDto(
    long Id,
    string Name,
    string Description,
    string Color,
    string? ImageUrl,
    decimal Price,
    int Stock,
    string? Url,
    decimal Cost)
    : CharmDto(Id, Name, Description, Color, ImageUrl, Price, Stock);

public static class CharmMappingExtensions
{
    public static CharmDto ToDto(this Charm c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.Color.ToString(),
        c.ImageUrl,
        c.Price,
        c.Stock);

    public static AdminCharmDto ToAdminDto(this Charm c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.Color.ToString(),
        c.ImageUrl,
        c.Price,
        c.Stock,
        c.Url,
        c.Cost);

    public static CharmDto ToDto(this Charm c, bool includeAdminFields) =>
        includeAdminFields ? c.ToAdminDto() : c.ToDto();
}
