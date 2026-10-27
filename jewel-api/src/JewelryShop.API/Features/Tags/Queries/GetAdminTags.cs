using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Queries;

public class GetAdminTagsQueryHandler : IRequestHandler<GetAdminTagsQuery, IReadOnlyList<AdminTagDto>>
{
    private readonly ITagRepository _tags;

    public GetAdminTagsQueryHandler(ITagRepository tags) => _tags = tags;

    public async Task<IReadOnlyList<AdminTagDto>> Handle(GetAdminTagsQuery request, CancellationToken cancellationToken)
    {
        var usages = await _tags.GetAllWithUsageAsync(cancellationToken);
        return usages.Select(u => u.ToAdminDto()).ToList();
    }
}
