namespace JewelryShop.Features.Orders.DTOs;

/// <summary>Commande dans la liste des commandes du client.</summary>
public record OrderSummaryDto(long Id, string Number, DateTime CreatedAt, string Status, decimal Total);

public static class OrderSummaryMappingExtensions
{
    public static OrderSummaryDto ToOrderSummaryDto(this Order order) =>
        new(order.Id, order.Number, order.CreatedAt, order.Status.ToString(), order.Total);
}
