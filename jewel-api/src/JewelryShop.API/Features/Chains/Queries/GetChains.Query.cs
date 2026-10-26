using JewelryShop.Common;
using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Queries;

public record GetChainsQuery(
    ProductColor? Color,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20,
    bool IncludeAdminFields = false,
    long? FavoritesFirstForUserId = null) : IRequest<PagedResult<ChainDto>>;
