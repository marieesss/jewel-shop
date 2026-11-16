namespace JewelryShop.Features.Orders.Entities;

/// <summary>Changement de statut d'une commande. <see cref="FromStatus"/> est vide pour la création de la commande.</summary>
public class OrderStatusHistory : BaseEntity
{
    public long OrderId { get; set; }
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public long ChangedBy { get; set; }
    public string? Message { get; set; }
    public bool CustomerNotified { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
}
