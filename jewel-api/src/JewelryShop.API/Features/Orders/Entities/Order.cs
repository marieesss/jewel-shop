namespace JewelryShop.Features.Orders.Entities;

/// <summary>
/// Commande d'un utilisateur. Le destinataire, l'adresse et le total sont copiés au moment de la commande :
/// ils ne changent pas si le profil ou le catalogue change ensuite.
/// </summary>
public class Order : BaseEntity
{
    public string Number { get; set; } = null!;
    public long UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? Comment { get; set; }
    public string RecipientName { get; set; } = null!;
    public string RecipientSurname { get; set; } = null!;
    public string RecipientPhone { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string PostalCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<OrderNote> Notes { get; set; } = new List<OrderNote>();
}
