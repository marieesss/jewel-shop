using JewelryShop.Common;
using JewelryShop.Features.Favorites.DTOs;
using MediatR;

namespace JewelryShop.Features.Favorites.Queries;

public record GetMyFavoritesQuery(long UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<FavoriteDto>>;
