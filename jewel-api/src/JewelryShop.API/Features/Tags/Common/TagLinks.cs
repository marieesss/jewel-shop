using JewelryShop.Features.Tags.DTOs;

namespace JewelryShop.Features.Tags.Common;

public static class TagLinks
{
    /// <summary>
    /// Remplace les tags d'un article : retire les liens absents de <paramref name="tags"/> et crée les manquants,
    /// sans toucher à ceux qui restent.
    /// </summary>
    public static void ReplaceWith<TLink>(this ICollection<TLink> links, IReadOnlyCollection<Tag> tags, Func<Tag, TLink> createLink)
        where TLink : ITagLink
    {
        var wantedIds = tags.Select(t => t.Id).ToHashSet();
        foreach (var removed in links.Where(l => !wantedIds.Contains(l.TagId)).ToList())
            links.Remove(removed);

        var currentIds = links.Select(l => l.TagId).ToHashSet();
        foreach (var added in tags.Where(t => !currentIds.Contains(t.Id)))
            links.Add(createLink(added));
    }

    /// <summary>Tags d'un article, triés par catégorie puis par nom. Les liens doivent être chargés avec leur tag.</summary>
    public static IReadOnlyList<TagDto> ToTagDtos(this IEnumerable<ITagLink> links)
        => links.Select(l => l.Tag).OrderByCategoryThenName(t => t).Select(t => t.ToDto()).ToList();
}
