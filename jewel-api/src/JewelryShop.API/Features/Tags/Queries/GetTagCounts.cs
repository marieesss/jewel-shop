using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Queries;

public class GetTagCountsQueryHandler : IRequestHandler<GetTagCountsQuery, IReadOnlyList<TagCountDto>>
{
    private readonly ITagRepository _tags;

    public GetTagCountsQueryHandler(ITagRepository tags) => _tags = tags;

    public async Task<IReadOnlyList<TagCountDto>> Handle(GetTagCountsQuery request, CancellationToken cancellationToken)
    {
        var usages = await _tags.GetAllWithUsageAsync(cancellationToken);
        return usages.Select(u => u.ToCountDto(CountFor(u, request.Kind))).ToList();
    }

    private static int CountFor(TagUsage usage, string kind)
        => kind == TagCountKinds.Chain ? usage.ChainCount : usage.CharmCount;
}
