using System.Text.Json.Serialization;

namespace JewelryShop.Features.Chains.DTOs;

/// <summary>Chaîne vue par un client : sans lien fournisseur ni coûts d'achat.</summary>
// Une AdminChainDto renvoyée là où une ChainDto est déclarée (liste paginée, query) garde ses
// champs admin à la sérialisation, au lieu d'être tronquée au type déclaré.
[JsonDerivedType(typeof(AdminChainDto))]
public record ChainDto(
    long Id,
    string Name,
    string Description,
    string Color,
    string? ImageUrl,
    decimal? Price,
    decimal? Length,
    decimal? PricePerMeter,
    decimal ClaspPrice,
    int StockCm,
    IReadOnlyList<ChainLengthPriceDto>? AvailableLengths = null,
    decimal? FromPrice = null);

public record ChainLengthPriceDto(int LengthCm, string Label, decimal? Price, bool Available);

/// <summary>Chaîne vue par un admin : ajoute le lien fournisseur et les coûts d'achat.</summary>
public record AdminChainDto(
    long Id,
    string Name,
    string Description,
    string Color,
    string? ImageUrl,
    decimal? Price,
    decimal? Length,
    decimal? PricePerMeter,
    decimal ClaspPrice,
    int StockCm,
    string? Url,
    decimal? Cost,
    decimal? CostPerMeter,
    decimal ClaspCost)
    : ChainDto(Id, Name, Description, Color, ImageUrl, Price, Length, PricePerMeter, ClaspPrice, StockCm);

public static class ChainMappingExtensions
{
    public static ChainDto ToDto(this Chain c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.Color.ToString(),
        c.ImageUrl,
        c.Price,
        c.Length,
        c.PricePerMeter,
        c.ClaspPrice,
        c.StockCm);

    public static AdminChainDto ToAdminDto(this Chain c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.Color.ToString(),
        c.ImageUrl,
        c.Price,
        c.Length,
        c.PricePerMeter,
        c.ClaspPrice,
        c.StockCm,
        c.Url,
        c.Cost,
        c.CostPerMeter,
        c.ClaspCost);

    public static ChainDto ToDto(this Chain c, bool includeAdminFields) =>
        includeAdminFields ? c.ToAdminDto() : c.ToDto();

    /// <summary>Ajoute au DTO le prix et la disponibilité de chaque longueur de référence.</summary>
    public static ChainDto ToDto(this Chain c, bool includeAdminFields, IEnumerable<ChainLength> lengths) =>
        c.ToDto(includeAdminFields).WithLengths(c, lengths);

    /// <summary>Variante admin de <see cref="ToDto(Chain, bool, IEnumerable{ChainLength})"/>, avec les champs admin.</summary>
    public static AdminChainDto ToAdminDto(this Chain c, IEnumerable<ChainLength> lengths) =>
        c.ToAdminDto().WithLengths(c, lengths);

    private static T WithLengths<T>(this T dto, Chain c, IEnumerable<ChainLength> lengths) where T : ChainDto
    {
        var lengthPrices = lengths
            .OrderBy(l => l.LengthCm)
            .Select(l => new ChainLengthPriceDto(l.LengthCm, l.Label, c.PriceFor(l.LengthCm), c.CanSell(l.LengthCm)))
            .ToList();

        return dto with
        {
            AvailableLengths = lengthPrices,
            FromPrice = lengthPrices.Where(l => l.Available).Min(l => l.Price)
        };
    }
}
