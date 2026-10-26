using JewelryShop.Common;
using JewelryShop.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Features.Charms.Queries;

public record GetCharmsQuery(
    ProductColor? Color,
    bool? InStockOnly,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20,
    bool IncludeAdminFields = false,
    long? FavoritesFirstForUserId = null) : IRequest<PagedResult<CharmDto>>;
