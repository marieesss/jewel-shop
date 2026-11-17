using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, AdminOrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;

    public GetOrderByIdQueryHandler(IOrderRepository orders, IUserRepository users)
    {
        _orders = orders;
        _users = users;
    }

    public async Task<AdminOrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orders.GetByIdWithDetailsAndNotesAsNoTrackingAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(OrderMessages.NotFound);

        var users = (await _users.GetByIdsAsNoTrackingAsync(order.RelatedUserIds(), cancellationToken))
            .ToDictionary(u => u.Id);

        return order.ToAdminOrderDto(users);
    }
}
