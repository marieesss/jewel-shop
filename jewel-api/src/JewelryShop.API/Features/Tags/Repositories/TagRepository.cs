using JewelryShop.Common.Persistence;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Features.Tags.Repositories;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(AppDbContext context) : base(context)
    {
    }

    // Le tri se fait en mémoire : la catégorie est stockée sous son nom, et l'ordre attendu est celui de l'enum.
    public async Task<IReadOnlyList<Tag>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        var tags = await DbSet.AsNoTracking().ToListAsync(cancellationToken);
        return tags.OrderByCategoryThenName(t => t).ToList();
    }

    public async Task<IReadOnlyList<TagUsage>> GetAllWithUsageAsync(CancellationToken cancellationToken = default)
    {
        var usages = await DbSet.AsNoTracking()
            .Select(t => new TagUsage(t, t.ChainTags.Count, t.CharmTags.Count))
            .ToListAsync(cancellationToken);

        return usages.OrderByCategoryThenName(u => u.Tag).ToList();
    }

    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default)
        => await DbSet.Where(t => ids.Contains(t.Id)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Tag>> GetBySlugsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(t => slugs.Contains(t.Slug)).ToListAsync(cancellationToken);

    public Task<Tag?> FindConflictingAsync(TagCategory category, string name, string slug, long? excludedId = null, CancellationToken cancellationToken = default)
    {
        var loweredName = name.ToLower();

        return DbSet.AsNoTracking().FirstOrDefaultAsync(
            t => t.Category == category && t.Id != excludedId && (t.Name.ToLower() == loweredName || t.Slug == slug),
            cancellationToken);
    }

    public Task MergeIntoAsync(long sourceId, long targetId, CancellationToken cancellationToken = default)
    {
        // EnableRetryOnFailure interdit une transaction ouverte à la main hors de la stratégie d'exécution.
        var strategy = Context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(cancellationToken);

            await Context.Database.ExecuteSqlAsync($"""
                INSERT INTO chain_tags (chain_id, tag_id)
                SELECT chain_id, {targetId} FROM chain_tags WHERE tag_id = {sourceId}
                ON CONFLICT DO NOTHING
                """, cancellationToken);
            await Context.Database.ExecuteSqlAsync($"""
                INSERT INTO charm_tags (charm_id, tag_id)
                SELECT charm_id, {targetId} FROM charm_tags WHERE tag_id = {sourceId}
                ON CONFLICT DO NOTHING
                """, cancellationToken);
            await Context.Database.ExecuteSqlAsync($"DELETE FROM tags WHERE id = {sourceId}", cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
