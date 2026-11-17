namespace JewelryShop.Features.Orders.Common;

/// <summary>Remise en stock des articles d'une commande annulée, par le client ou par un admin.</summary>
public static class OrderStock
{
    /// <summary>
    /// Remet la longueur de la chaîne dans le rouleau où elle a été prise, puis les breloques dans leur stock.
    /// Même ordre de verrouillage que la prise du stock (le rouleau, puis les breloques par id), pour qu'une
    /// annulation et une commande simultanées ne s'attendent pas mutuellement.
    /// </summary>
    public static async Task ReturnAsync(IOrderRepository orders, Order order, CancellationToken cancellationToken)
    {
        // Une ligne de chaîne sans rouleau n'a rien à recréditer dans un rouleau.
        var chainLine = order.Lines.SingleOrDefault(l => l.Kind == OrderLineKind.Chain);
        if (chainLine is { ChainRollId: { } rollId, LengthCm: { } lengthCm })
            await orders.ReturnChainLengthAsync(rollId, lengthCm, cancellationToken);

        var charmCopies = order.Lines
            .Where(l => l.Kind == OrderLineKind.Charm && l.CharmId is not null)
            .GroupBy(l => l.CharmId!.Value)
            .OrderBy(g => g.Key);
        foreach (var copies in charmCopies)
            await orders.ReturnCharmStockAsync(copies.Key, copies.Count(), cancellationToken);
    }
}
