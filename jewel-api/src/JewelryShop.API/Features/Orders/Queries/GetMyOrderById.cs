using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

public class GetMyOrderByIdQueryHandler : IRequestHandler<GetMyOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _orders;

    public GetMyOrderByIdQueryHandler(IOrderRepository orders) => _orders = orders;

    public async Task<OrderDto> Handle(GetMyOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orders.GetCustomerOrderAsNoTrackingAsync(request.UserId, request.OrderId, cancellationToken);
        return order.ToOrderDto();
    }
}
