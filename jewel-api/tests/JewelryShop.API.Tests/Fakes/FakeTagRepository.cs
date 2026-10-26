using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.Entities;
using JewelryShop.Features.Tags.Repositories;

namespace JewelryShop.API.Tests.Fakes;

public class FakeTagRepository : InMemoryRepository<Tag>, ITagRepository
{
    public Exception? SaveChangesError { get; set; }

    public Task<IReadOnlyList<Tag>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Tag>>(Entities.OrderByCategoryThenName(t => t).ToList());

    public Task<IReadOnlyList<TagUsage>> GetAllWithUsageAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TagUsage>>(Entities
            .Select(t => new TagUsage(t, t.ChainTags.Count, t.CharmTags.Count))
            .OrderByCategoryThenName(u => u.Tag)
            .ToList());

    public Task<Tag?> FindConflictingAsync(TagCategory category, string name, string slug, long? excludedId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Entities.FirstOrDefault(t =>
            t.Category == category
            && t.Id != excludedId
            && (string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase) || t.Slug == slug)));

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesError is null ? base.SaveChangesAsync(cancellationToken) : Task.FromException<int>(SaveChangesError);
}
