namespace JewelryShop.Features.Tags.Entities;

public class ChainTag : ITagLink
{
    public long ChainId { get; set; }
    public long TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
