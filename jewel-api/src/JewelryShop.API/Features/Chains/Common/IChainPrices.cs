namespace JewelryShop.Features.Chains.Common;

/// <summary>
/// Prix saisis à la création ou à la modification d'une chaîne, dans l'un des deux formats :
/// l'ancien (<see cref="Cost"/>, <see cref="Price"/> pour <see cref="Length"/> cm), encore envoyé
/// par le front en production, ou le nouveau (prix au mètre), qui prime dès qu'il est fourni.
/// </summary>
public interface IChainPrices
{
    decimal? Cost { get; }
    decimal? Price { get; }
    decimal? Length { get; }
    decimal? CostPerMeter { get; }
    decimal? PricePerMeter { get; }
    decimal? ClaspCost { get; }
    decimal? ClaspPrice { get; }
}

public static class ChainPricesExtensions
{
    public static bool UsesMeterPrices(this IChainPrices prices) =>
        prices.CostPerMeter.HasValue || prices.PricePerMeter.HasValue;

    public static bool HasCompleteLegacyPrices(this IChainPrices prices) =>
        prices.Cost.HasValue && prices.Price.HasValue && prices.Length.HasValue;
}
