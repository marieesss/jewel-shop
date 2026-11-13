namespace JewelryShop.Features.Orders.Common;

/// <summary>Changements de statut autorisés d'une commande, selon qui les demande.</summary>
public static class OrderStateMachine
{
    /// <summary>
    /// Le client peut seulement annuler une commande en validation et confirmer la réception d'une commande envoyée.
    /// L'admin passe librement d'un statut actif à un autre et peut annuler tant que la commande n'est pas reçue.
    /// Une commande annulée ne change plus de statut, et rester sur le même statut n'est pas une transition.
    /// </summary>
    public static bool CanTransition(OrderStatus from, OrderStatus to, OrderActor actor) => actor switch
    {
        OrderActor.Customer => CustomerCanTransition(from, to),
        OrderActor.Admin => AdminCanTransition(from, to),
        _ => false
    };

    /// <summary>Le stock est recrédité à l'annulation tant que la commande n'est pas partie.</summary>
    public static bool RestocksOnCancellation(OrderStatus from) =>
        from is OrderStatus.Pending or OrderStatus.Processing;

    private static bool CustomerCanTransition(OrderStatus from, OrderStatus to) =>
        (from, to) is (OrderStatus.Pending, OrderStatus.Cancelled) or (OrderStatus.Shipped, OrderStatus.Received);

    private static bool AdminCanTransition(OrderStatus from, OrderStatus to)
    {
        var isFinal = from == OrderStatus.Cancelled;
        var cancelsReceivedOrder = from == OrderStatus.Received && to == OrderStatus.Cancelled;
        return from != to && !isFinal && !cancelsReceivedOrder;
    }
}
