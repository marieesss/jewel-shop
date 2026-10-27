using FluentValidation.Results;
using JewelryShop.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JewelryShop.Features.Tags.Common;

public static class TagRepositoryExtensions
{
    /// <summary>
    /// Refuse un nom déjà pris dans la catégorie (sans tenir compte de la casse), ou un nom différent
    /// qui donnerait le slug d'un tag existant (« Dore » face à « Doré »).
    /// </summary>
    public static async Task EnsureNameIsAvailableAsync(this ITagRepository tags, TagCategory category, string name, long? excludedId, CancellationToken cancellationToken)
    {
        var existing = await tags.FindConflictingAsync(category, name, TagSlug.From(name), excludedId, cancellationToken);
        if (existing is null)
            return;

        throw new ConflictException(string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase)
            ? TagMessages.AlreadyExists(name)
            : TagMessages.TooCloseTo(existing.Name));
    }

    /// <summary>
    /// Les tags demandés pour un article, sans doublon. Un id inconnu donne une erreur de validation
    /// « Tag introuvable. » sous la clé <paramref name="propertyName"/>.
    /// </summary>
    public static async Task<IReadOnlyList<Tag>> GetForProductAsync(this ITagRepository tags, IEnumerable<long> ids, string propertyName, CancellationToken cancellationToken)
    {
        var distinctIds = ids.Distinct().ToList();
        var found = await tags.GetByIdsAsync(distinctIds, cancellationToken);
        if (found.Count != distinctIds.Count)
            throw new ValidationException([new ValidationFailure(propertyName, TagMessages.NotFound)]);

        return found;
    }

    // Le contrôle EnsureNameIsAvailableAsync n'est pas atomique : deux requêtes simultanées peuvent
    // le passer, et les index uniques de la catégorie refusent la seconde à l'enregistrement.
    public static async Task SaveChangesOrConflictAsync(this ITagRepository tags, string name, CancellationToken cancellationToken)
    {
        try
        {
            await tags.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException(TagMessages.AlreadyExists(name));
        }
    }
}
