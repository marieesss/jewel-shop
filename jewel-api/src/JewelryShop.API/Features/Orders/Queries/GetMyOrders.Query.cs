using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

/// <summary>Commandes de l'utilisateur, des plus récentes aux plus anciennes.</summary>
public record GetMyOrdersQuery(
    long UserId,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize) : IRequest<PagedResult<OrderSummaryDto>>;
