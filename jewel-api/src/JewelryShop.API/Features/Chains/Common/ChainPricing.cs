namespace JewelryShop.Features.Chains.Common;

public static class ChainPricing
{
    public const int CentimetersPerMeter = 100;

    /// <summary>
    /// Montant au mètre d'une chaîne vendue <paramref name="total"/> pour <paramref name="lengthCm"/> cm,
    /// arrondi au centime le plus proche (au pair en cas d'égalité, comme la migration V7).
    /// </summary>
    public static decimal PerMeter(decimal total, decimal lengthCm) =>
        Math.Round(total * CentimetersPerMeter / lengthCm, 2);

    /// <summary>
    /// Longueur en cm entiers du rouleau correspondant à une ancienne longueur décimale,
    /// arrondie au demi supérieur comme le ROUND de la migration V7.
    /// </summary>
    public static int RollLengthCm(decimal lengthCm) =>
        (int)Math.Round(lengthCm, MidpointRounding.AwayFromZero);
}
