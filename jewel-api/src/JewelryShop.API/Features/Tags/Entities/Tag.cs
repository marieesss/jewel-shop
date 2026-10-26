using JewelryShop.Features.Tags.Common;

namespace JewelryShop.Features.Tags.Entities;

/// <summary>Tag qui classe les chaînes et les breloques. Son slug suit son nom, y compris au renommage.</summary>
public class Tag : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public TagCategory Category { get; private set; }

    public ICollection<ChainTag> ChainTags { get; private set; } = new List<ChainTag>();
    public ICollection<CharmTag> CharmTags { get; private set; } = new List<CharmTag>();

    private Tag()
    {
    }

    public Tag(string name, TagCategory category)
    {
        Category = category;
        Rename(name);
    }

    public void Rename(string name)
    {
        Name = name.Trim();
        Slug = TagSlug.From(Name);
    }
}
