namespace JewelryShop.Features.Tags.Common;

/// <summary>
/// Filtre du catalogue par tags : un article doit porter au moins un tag de chaque groupe (OU dans une catégorie,
/// ET entre catégories). Sans groupe, le filtre ne retient rien.
/// </summary>
public record TagFilter(IReadOnlyList<IReadOnlyList<long>> TagIdGroups)
{
    public static readonly TagFilter None = new([]);

    public bool IsEmpty => TagIdGroups.Count == 0;

    /// <summary>Un groupe par catégorie des tags reçus, dans l'ordre de <see cref="TagCategory"/>.</summary>
    public static TagFilter From(IEnumerable<Tag> tags) => new(tags
        .GroupBy(t => t.Category)
        .OrderBy(g => g.Key)
        .Select(g => (IReadOnlyList<long>)g.Select(t => t.Id).Distinct().Order().ToList())
        .ToList());

    /// <summary>
    /// Slugs d'un paramètre <c>tags</c> séparés par des virgules (« mer,plage,dore »). Chaque valeur est ramenée à un slug,
    /// pour qu'un nom saisi tel quel (« Doré ») soit reconnu ; les valeurs vides et les doublons sont retirés.
    /// </summary>
    public static IReadOnlyList<string> ParseSlugs(string? tags)
        => string.IsNullOrWhiteSpace(tags)
            ? []
            : tags.Split(',').Select(TagSlug.From).Where(s => s.Length > 0).Distinct().ToList();
}
