using JewelryShop.Common.Models;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<AdminOrderSummaryDto>>
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;

    public GetOrdersQueryHandler(IOrderRepository orders, IUserRepository users)
    {
        _orders = orders;
        _users = users;
    }

    public async Task<PagedResult<AdminOrderSummaryDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Le validateur a déjà refusé un statut inconnu.
        OrderStatusFilter.TryParse(request.Status, out var status);
        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

        var (items, total) = await _orders.GetPagedAsync(status, page, pageSize, cancellationToken);

        var customerIds = items.Select(o => o.UserId).ToHashSet();
        var customers = (await _users.GetByIdsAsNoTrackingAsync(customerIds, cancellationToken))
            .ToDictionary(u => u.Id);

        var dtos = items.Select(o => o.ToAdminOrderSummaryDto(customers[o.UserId])).ToList();
        return new PagedResult<AdminOrderSummaryDto>(dtos, total, page, pageSize);
    }
}
