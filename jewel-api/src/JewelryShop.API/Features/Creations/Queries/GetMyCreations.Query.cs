using JewelryShop.Common;
using JewelryShop.Features.Creations.DTOs;
using MediatR;

namespace JewelryShop.Features.Creations.Queries;

public record GetMyCreationsQuery(long UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<CreationDto>>;
