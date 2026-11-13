namespace JewelryShop.Features.Orders.Common;

/// <summary>Numéro lisible d'une commande, par exemple <c>CMD-2026-0042</c>.</summary>
public static class OrderNumber
{
    private const string Prefix = "CMD";

    /// <summary>
    /// Assemble l'année de création et la valeur de la séquence <c>order_number_seq</c>, sur 4 chiffres au moins.
    /// La séquence ne repart pas à 1 au changement d'année : <c>CMD-2026-0042</c> est suivi de <c>CMD-2027-0043</c>.
    /// </summary>
    public static string Format(DateTime createdAt, long sequenceValue) =>
        $"{Prefix}-{createdAt.Year}-{sequenceValue:D4}";
}
