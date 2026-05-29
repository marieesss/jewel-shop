using JewelryShop.Application.Common;
using JewelryShop.Application.Common.Models;
using JewelryShop.Application.Features.Creations.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Creations.Queries;

public record GetMyCreationsQuery(long UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<CreationDto>>;

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
