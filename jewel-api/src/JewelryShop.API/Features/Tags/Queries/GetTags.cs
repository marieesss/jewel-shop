using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Queries;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, IReadOnlyList<TagDto>>
{
    private readonly ITagRepository _tags;

    public GetTagsQueryHandler(ITagRepository tags) => _tags = tags;

    public async Task<IReadOnlyList<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _tags.GetAllOrderedAsync(cancellationToken);
        return tags.Select(t => t.ToDto()).ToList();
    }
}
