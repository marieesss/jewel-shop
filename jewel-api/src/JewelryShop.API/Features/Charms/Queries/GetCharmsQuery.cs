using JewelryShop.Application.Common;
using JewelryShop.Application.Common.Models;
using JewelryShop.Application.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Charms.Queries;

public record GetCharmsQuery(
    ProductColor? Color,
    bool? InStockOnly,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CharmDto>>;

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

        var dtos = items.Select(c => c.ToDto()).ToList();
        return new PagedResult<CharmDto>(dtos, total, page, pageSize);
    }
}
