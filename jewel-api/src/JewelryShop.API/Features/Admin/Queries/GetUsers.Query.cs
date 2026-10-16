using JewelryShop.Common;
using JewelryShop.Features.Users.DTOs;
using MediatR;

namespace JewelryShop.Features.Admin.Queries;

public record GetUsersQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<UserDto>>;
