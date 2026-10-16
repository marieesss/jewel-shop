namespace JewelryShop.Application.Features.Creations;

public class CreationItem : BaseEntity
{
    public long CreationId { get; set; }
    public long CharmId { get; set; }
    public int Position { get; set; }

    public Creation Creation { get; set; } = null!;
    public Charm Charm { get; set; } = null!;
}
