using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Charms.DTOs;
using JewelryShop.Features.Tags.Common;
using MediatR;

namespace JewelryShop.Features.Charms.Queries;

public class GetCharmsQueryHandler : IRequestHandler<GetCharmsQuery, PagedResult<CharmDto>>
{
    private readonly ICharmRepository _charms;
    private readonly ITagRepository _tags;

    public GetCharmsQueryHandler(ICharmRepository charms, ITagRepository tags)
    {
        _charms = charms;
        _tags = tags;
    }

    public async Task<PagedResult<CharmDto>> Handle(GetCharmsQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);
        var tagFilter = await _tags.GetCatalogFilterAsync(request.Tags, cancellationToken);

        var (items, total) = await _charms.GetPagedAsync(
            request.Color, request.InStockOnly, request.MinPrice, request.MaxPrice, tagFilter,
            page, pageSize, request.FavoritesFirstForUserId, cancellationToken);

        var dtos = items.Select(c => c.ToDto(request.IncludeAdminFields)).ToList();
        return new PagedResult<CharmDto>(dtos, total, page, pageSize);
    }
}
