using JewelryShop.Features.Tags.Common;

namespace JewelryShop.Features.Charms.Entities;

public class Charm : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public ProductColor Color { get; set; }
    public string? ImageUrl { get; set; }
    public string? Url { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<CreationItem> CreationItems { get; set; } = new List<CreationItem>();
    public ICollection<CharmTag> CharmTags { get; set; } = new List<CharmTag>();

    public void ReplaceTags(IReadOnlyCollection<Tag> tags) =>
        CharmTags.ReplaceWith(tags, tag => new CharmTag { CharmId = Id, TagId = tag.Id, Tag = tag });
}
