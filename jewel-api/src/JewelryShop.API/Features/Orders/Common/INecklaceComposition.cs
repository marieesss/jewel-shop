namespace JewelryShop.Features.Orders.Common;

/// <summary>
/// Collier demandé par le client, commun au devis et à la commande. L'ordre de <see cref="CharmIds"/>
/// donne la position de chaque breloque (de 1 à <see cref="NecklaceRules.MaxCharms"/>), et un même id
/// répété demande plusieurs exemplaires de la breloque.
/// </summary>
public interface INecklaceComposition
{
    long ChainId { get; }
    int LengthCm { get; }
    IReadOnlyList<long> CharmIds { get; }
}
