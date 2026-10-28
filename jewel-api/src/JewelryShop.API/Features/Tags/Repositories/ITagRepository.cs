using JewelryShop.Common.Persistence;

namespace JewelryShop.Features.Tags.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    /// <summary>Tous les tags, par catégorie (dans l'ordre de <see cref="TagCategory"/>) puis par nom.</summary>
    Task<IReadOnlyList<Tag>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

    /// <summary>Tous les tags avec le nombre d'articles qui les portent, dans le même ordre que <see cref="GetAllOrderedAsync"/>.</summary>
    Task<IReadOnlyList<TagUsage>> GetAllWithUsageAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Les tags de ces ids qui existent, suivis par le contexte : ils peuvent être associés à un article enregistré ensuite.
    /// </summary>
    Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default);

    /// <summary>Les tags dont le slug est dans <paramref name="slugs"/>, toutes catégories confondues. Un slug inconnu ne renvoie rien.</summary>
    Task<IReadOnlyList<Tag>> GetBySlugsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Un autre tag que <paramref name="excludedId"/> de la catégorie dont le nom (sans tenir compte de la casse)
    /// ou le slug est déjà pris, ou null.
    /// </summary>
    Task<Tag?> FindConflictingAsync(TagCategory category, string name, string slug, long? excludedId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dans une seule transaction, reporte les chaînes et les breloques du tag <paramref name="sourceId"/> sur
    /// <paramref name="targetId"/> (un article qui porte déjà le tag cible le garde une seule fois), puis supprime le tag source.
    /// </summary>
    Task MergeIntoAsync(long sourceId, long targetId, CancellationToken cancellationToken = default);
}
