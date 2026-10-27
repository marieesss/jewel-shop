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

    public Task<Tag?> FindConflictingAsync(TagCategory category, string name, string slug, long? excludedId = null, CancellationToken cancellationToken = default)
    {
        var loweredName = name.ToLower();

        return DbSet.AsNoTracking().FirstOrDefaultAsync(
            t => t.Category == category && t.Id != excludedId && (t.Name.ToLower() == loweredName || t.Slug == slug),
            cancellationToken);
    }
}
