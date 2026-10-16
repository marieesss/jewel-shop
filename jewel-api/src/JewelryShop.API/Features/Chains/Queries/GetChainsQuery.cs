using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Queries;

public record GetChainsQuery(
    ProductColor? Color,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ChainDto>>;

public class GetChainsQueryHandler : IRequestHandler<GetChainsQuery, PagedResult<ChainDto>>
{
    private readonly IChainRepository _chains;

    public GetChainsQueryHandler(IChainRepository chains) => _chains = chains;

    public async Task<PagedResult<ChainDto>> Handle(GetChainsQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

        var (items, total) = await _chains.GetPagedAsync(
            request.Color, request.MinPrice, request.MaxPrice, page, pageSize, cancellationToken);

        var dtos = items.Select(c => c.ToDto()).ToList();
        return new PagedResult<ChainDto>(dtos, total, page, pageSize);
    }
}
