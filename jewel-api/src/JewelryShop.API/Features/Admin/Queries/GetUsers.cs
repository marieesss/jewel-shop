using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Users.DTOs;
using MediatR;

namespace JewelryShop.Features.Admin.Queries;

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
