using JewelryShop.Common;
using JewelryShop.Common.Models;
using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IOrderRepository _orders;

    public GetMyOrdersQueryHandler(IOrderRepository orders) => _orders = orders;

    public async Task<PagedResult<OrderSummaryDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

        var (items, total) = await _orders.GetPagedByUserAsync(request.UserId, page, pageSize, cancellationToken);

        var dtos = items.Select(o => o.ToOrderSummaryDto()).ToList();
        return new PagedResult<OrderSummaryDto>(dtos, total, page, pageSize);
    }
}
