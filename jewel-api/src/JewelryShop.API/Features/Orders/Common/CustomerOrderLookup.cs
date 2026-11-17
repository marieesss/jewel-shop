using JewelryShop.Common.Exceptions;

namespace JewelryShop.Features.Orders.Common;

public static class CustomerOrderLookup
{
    /// <summary>
    /// Commande du client avec ses lignes et son historique. La commande d'un autre utilisateur est introuvable,
    /// comme une commande qui n'existe pas : le client n'apprend pas qu'elle existe.
    /// </summary>
    public static async Task<Order> GetCustomerOrderAsNoTrackingAsync(
        this IOrderRepository orders, long userId, long orderId, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdWithDetailsAsNoTrackingAsync(orderId, cancellationToken);
        return order is not null && order.UserId == userId
            ? order
            : throw new NotFoundException(OrderMessages.NotFound);
    }
}
