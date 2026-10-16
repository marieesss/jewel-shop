using JewelryShop.Common;
using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Models;
using JewelryShop.Features.Favorites.DTOs;
using MediatR;

namespace JewelryShop.Features.Admin.Queries;

public class GetUserFavoritesQueryHandler : IRequestHandler<GetUserFavoritesQuery, PagedResult<FavoriteDto>>
{
    private readonly IFavoriteRepository _favorites;
    private readonly IUserRepository _users;

    public GetUserFavoritesQueryHandler(IFavoriteRepository favorites, IUserRepository users)
    {
        _favorites = favorites;
        _users = users;
    }

    public async Task<PagedResult<FavoriteDto>> Handle(GetUserFavoritesQuery request, CancellationToken cancellationToken)
    {
        if (await _users.GetByIdAsync(request.UserId, cancellationToken) is null)
            throw new NotFoundException("Utilisateur", request.UserId);

        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

        var (items, total) = await _favorites.GetPagedByUserAsync(
            request.UserId, page, pageSize, cancellationToken);

        var dtos = items.Select(f => f.ToDto()).ToList();
        return new PagedResult<FavoriteDto>(dtos, total, page, pageSize);
    }
}
