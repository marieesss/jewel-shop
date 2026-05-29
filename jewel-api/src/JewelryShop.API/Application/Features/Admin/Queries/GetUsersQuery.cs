using JewelryShop.Application.Common;
using JewelryShop.Application.Common.Models;
using JewelryShop.Application.Features.Users;
using JewelryShop.Application.Features.Users.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Admin.Queries;

public record GetUsersQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserRepository _users;

    public GetUsersQueryHandler(IUserRepository users) => _users = users;

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

        var (items, total) = await _users.GetPagedAsync(page, pageSize, cancellationToken);

        var dtos = items.Select(u => u.ToDto()).ToList();
        return new PagedResult<UserDto>(dtos, total, page, pageSize);
    }
}
