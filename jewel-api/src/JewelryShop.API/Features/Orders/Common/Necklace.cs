namespace JewelryShop.Features.Orders.Common;

/// <summary>
/// Collier dont les produits existent, dont la longueur est proposée et dont le stock suffit.
/// <see cref="Charms"/> suit l'ordre demandé : la breloque d'index i est en position i + 1.
/// </summary>
public sealed record Necklace(Chain Chain, int LengthCm, IReadOnlyList<Charm> Charms)
{
    /// <summary>Prix de la chaîne coupée à la longueur, fermoir compris. Une chaîne vendable a toujours un prix au mètre.</summary>
    public decimal ChainPrice => Chain.PriceFor(LengthCm)!.Value;

    public decimal Total => ChainPrice + Charms.Sum(c => c.Price);
}
