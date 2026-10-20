using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Features.Charms.Queries;

public class GetCharmsQueryHandler : IRequestHandler<GetCharmsQuery, PagedResult<CharmDto>>
{
    private readonly ICharmRepository _charms;

    public GetCharmsQueryHandler(ICharmRepository charms) => _charms = charms;

    public async Task<PagedResult<CharmDto>> Handle(GetCharmsQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

        var (items, total) = await _charms.GetPagedAsync(
            request.Color, request.InStockOnly, request.MinPrice, request.MaxPrice,
            page, pageSize, cancellationToken);

        var dtos = items.Select(c => c.ToDto(request.IncludeAdminFields)).ToList();
        return new PagedResult<CharmDto>(dtos, total, page, pageSize);
    }
}
