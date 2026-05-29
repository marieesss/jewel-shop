namespace JewelryShop.Application.Features.Creations;

public class Creation : BaseEntity
{
    public long UserId { get; set; }
    public long ChainId { get; set; }
    public long OptionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Chain Chain { get; set; } = null!;
    public Option Option { get; set; } = null!;
    public ICollection<CreationItem> Items { get; set; } = new List<CreationItem>();
}
