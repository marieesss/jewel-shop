namespace JewelryShop.Features.Orders.Entities;

/// <summary>Note interne d'un admin sur une commande.</summary>
public class OrderNote : BaseEntity
{
    public long OrderId { get; set; }
    public long AuthorId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Order Order { get; set; } = null!;
}
