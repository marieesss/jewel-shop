using JewelryShop.Application.Common;
using JewelryShop.Application.Common.Models;
using JewelryShop.Application.Features.Favorites.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Favorites.Queries;

public record GetMyFavoritesQuery(long UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<FavoriteDto>>;

public class GetMyFavoritesQueryHandler : IRequestHandler<GetMyFavoritesQuery, PagedResult<FavoriteDto>>
{
    private readonly IFavoriteRepository _favorites;

    public GetMyFavoritesQueryHandler(IFavoriteRepository favorites) => _favorites = favorites;

    public async Task<PagedResult<FavoriteDto>> Handle(GetMyFavoritesQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

        var (items, total) = await _favorites.GetPagedByUserAsync(
            request.UserId, page, pageSize, cancellationToken);

        var dtos = items.Select(f => f.ToDto()).ToList();
        return new PagedResult<FavoriteDto>(dtos, total, page, pageSize);
    }
}
