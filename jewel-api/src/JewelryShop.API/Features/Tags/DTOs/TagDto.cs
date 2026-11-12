using JewelryShop.Features.Tags.Repositories;

namespace JewelryShop.Features.Tags.DTOs;

public record TagDto(long Id, string Name, string Slug, string Category);

/// <summary>Tag avec le nombre global d'articles d'un type (chaînes ou breloques) qui le portent.</summary>
public record TagCountDto(long Id, string Name, string Slug, string Category, int Count);

public record AdminTagDto(long Id, string Name, string Slug, string Category, int ChainCount, int CharmCount);

public static class TagMappingExtensions
{
    public static TagDto ToDto(this Tag tag) => new(tag.Id, tag.Name, tag.Slug, tag.Category.ToString());

    public static TagCountDto ToCountDto(this TagUsage usage, int count)
        => new(usage.Tag.Id, usage.Tag.Name, usage.Tag.Slug, usage.Tag.Category.ToString(), count);

    public static AdminTagDto ToAdminDto(this TagUsage usage) => new(
        usage.Tag.Id,
        usage.Tag.Name,
        usage.Tag.Slug,
        usage.Tag.Category.ToString(),
        usage.ChainCount,
        usage.CharmCount);
}
