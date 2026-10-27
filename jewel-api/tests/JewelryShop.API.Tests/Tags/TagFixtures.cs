using JewelryShop.Features.Tags.Entities;

namespace JewelryShop.API.Tests.Tags;

public static class TagFixtures
{
    public static Tag Tag(long id, string name, TagCategory category = TagCategory.Theme) => new(name, category) { Id = id };
}
