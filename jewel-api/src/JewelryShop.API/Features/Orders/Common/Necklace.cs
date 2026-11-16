namespace JewelryShop.Features.Orders.Common;

/// <summary>
/// Collier dont les produits existent, dont la longueur est proposée et dont le stock suffit.
/// <see cref="Charms"/> suit l'ordre demandé : la breloque d'index i est en position i + 1.
/// </summary>
public sealed class Necklace
{
    /// <exception cref="ArgumentException">La chaîne ne peut pas être vendue à cette longueur (sans prix au mètre ou sans rouleau assez long).</exception>
    public Necklace(Chain chain, int lengthCm, IReadOnlyList<Charm> charms)
    {
        ChainPrice = chain.CanSell(lengthCm)
            ? chain.PriceFor(lengthCm)!.Value
            : throw new ArgumentException($"La chaîne « {chain.Name} » ne peut pas être vendue à {lengthCm} cm.", nameof(chain));

        Chain = chain;
        LengthCm = lengthCm;
        Charms = charms;
    }

    public Chain Chain { get; }

    public int LengthCm { get; }

    public IReadOnlyList<Charm> Charms { get; }

    /// <summary>Prix de la chaîne coupée à la longueur, fermoir compris.</summary>
    public decimal ChainPrice { get; }

    public decimal Total => ChainPrice + Charms.Sum(c => c.Price);
}
