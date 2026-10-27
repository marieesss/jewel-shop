namespace JewelryShop.Features.Tags.Entities;

public class CharmTag : ITagLink
{
    public long CharmId { get; set; }
    public long TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
