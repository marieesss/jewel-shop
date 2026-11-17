namespace JewelryShop.Features.Orders.Common;

/// <summary>Filtre par statut de la liste admin des commandes.</summary>
public static class OrderStatusFilter
{
    /// <summary>
    /// Lit le statut demandé : vide pour tous les statuts, sinon le nom d'un statut sans tenir compte de la casse.
    /// Une valeur numérique est refusée.
    /// </summary>
    public static bool TryParse(string? value, out OrderStatus? status)
    {
        status = null;
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var name = Enum.GetNames<OrderStatus>()
            .FirstOrDefault(n => string.Equals(n, value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (name is null)
            return false;

        status = Enum.Parse<OrderStatus>(name);
        return true;
    }
}
