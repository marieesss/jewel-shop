using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Creations.DTOs;
using MediatR;

namespace JewelryShop.Features.Creations.Queries;

public class GetMyCreationsQueryHandler : IRequestHandler<GetMyCreationsQuery, PagedResult<CreationDto>>
{
    private readonly ICreationRepository _creations;

    public GetMyCreationsQueryHandler(ICreationRepository creations) => _creations = creations;

    public async Task<PagedResult<CreationDto>> Handle(GetMyCreationsQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

        var (items, total) = await _creations.GetPagedByUserAsync(
            request.UserId, page, pageSize, cancellationToken);

        var dtos = items.Select(c => c.ToDto()).ToList();
        return new PagedResult<CreationDto>(dtos, total, page, pageSize);
    }
}
