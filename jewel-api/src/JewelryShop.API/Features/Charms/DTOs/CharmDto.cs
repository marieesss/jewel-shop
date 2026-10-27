using System.Text.Json.Serialization;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.DTOs;

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
    int Stock,
    IReadOnlyList<TagDto>? Tags = null);

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

    /// <summary>DTO du catalogue : ajoute les tags de la breloque, qui doivent être chargés avec elle.</summary>
    public static CharmDto ToDto(this Charm c, bool includeAdminFields) =>
        (includeAdminFields ? c.ToAdminDto() : c.ToDto()).WithTags(c);

    /// <summary>Variante admin de <see cref="ToDto(Charm, bool)"/>, renvoyée par la création et la modification.</summary>
    public static AdminCharmDto ToAdminDtoWithTags(this Charm c) => c.ToAdminDto().WithTags(c);

    private static T WithTags<T>(this T dto, Charm c) where T : CharmDto =>
        dto with { Tags = c.CharmTags.ToTagDtos() };
}
