using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Queries;

public class GetChainsQueryHandler : IRequestHandler<GetChainsQuery, PagedResult<ChainDto>>
{
    private readonly IChainRepository _chains;
    private readonly IChainLengthRepository _lengths;

    public GetChainsQueryHandler(IChainRepository chains, IChainLengthRepository lengths)
    {
        _chains = chains;
        _lengths = lengths;
    }

    public async Task<PagedResult<ChainDto>> Handle(GetChainsQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

        var (items, total) = await _chains.GetPagedAsync(
            request.Color, request.MinPrice, request.MaxPrice, page, pageSize,
            request.FavoritesFirstForUserId, cancellationToken);

        var lengths = await _lengths.GetAllOrderedAsync(cancellationToken);
        var dtos = items.Select(c => c.ToDto(request.IncludeAdminFields, lengths)).ToList();
        return new PagedResult<ChainDto>(dtos, total, page, pageSize);
    }
}
