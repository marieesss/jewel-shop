using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Chains.DTOs;
using JewelryShop.Features.Tags.Common;
using MediatR;

namespace JewelryShop.Features.Chains.Queries;

public class GetChainsQueryHandler : IRequestHandler<GetChainsQuery, PagedResult<ChainDto>>
{
    private readonly IChainRepository _chains;
    private readonly IChainLengthRepository _lengths;
    private readonly ITagRepository _tags;

    public GetChainsQueryHandler(IChainRepository chains, IChainLengthRepository lengths, ITagRepository tags)
    {
        _chains = chains;
        _lengths = lengths;
        _tags = tags;
    }

    public async Task<PagedResult<ChainDto>> Handle(GetChainsQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);
        var tagFilter = await _tags.GetCatalogFilterAsync(request.Tags, cancellationToken);

        var (items, total) = await _chains.GetPagedAsync(
            request.Color, request.MinPrice, request.MaxPrice, tagFilter, page, pageSize,
            request.FavoritesFirstForUserId, cancellationToken);

        var lengths = await _lengths.GetAllOrderedAsync(cancellationToken);
        var dtos = items.Select(c => c.ToDto(request.IncludeAdminFields, lengths)).ToList();
        return new PagedResult<ChainDto>(dtos, total, page, pageSize);
    }
}
