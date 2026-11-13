namespace JewelryShop.Features.Orders.Entities;

/// <summary>Statut d'une commande, stocké sous son nom. Les changements autorisés sont décrits par <c>OrderStateMachine</c>.</summary>
public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Received,
    Cancelled
}
