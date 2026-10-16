using JewelryShop.Application.Common;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Common.Models;
using JewelryShop.Application.Features.Favorites;
using JewelryShop.Application.Features.Favorites.DTOs;
using JewelryShop.Application.Features.Users;
using MediatR;

namespace JewelryShop.Application.Features.Admin.Queries;

public record GetUserFavoritesQuery(long UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<FavoriteDto>>;

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
