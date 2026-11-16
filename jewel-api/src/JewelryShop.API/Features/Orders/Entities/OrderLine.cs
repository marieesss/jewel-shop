namespace JewelryShop.Features.Orders.Entities;

/// <summary>
/// Ligne d'une commande : la chaîne (avec sa longueur et le rouleau où elle est prise) ou une breloque
/// (avec sa position). Le nom et les prix sont copiés au moment de la commande.
/// </summary>
public class OrderLine : BaseEntity
{
    public long OrderId { get; set; }
    public OrderLineKind Kind { get; set; }
    public long? ChainId { get; set; }
    public long? CharmId { get; set; }
    public long? ChainRollId { get; set; }
    public string Name { get; set; } = null!;
    public int? LengthCm { get; set; }
    public decimal? PricePerMeter { get; set; }
    public decimal? ClaspPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public int? Position { get; set; }

    public Order Order { get; set; } = null!;
}
