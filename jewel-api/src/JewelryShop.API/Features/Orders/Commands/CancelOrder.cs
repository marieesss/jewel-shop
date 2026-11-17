using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Infrastructure.Email;
using MediatR;

namespace JewelryShop.Features.Orders.Commands;

/// <summary>Passe la commande <c>Pending</c> en <c>Cancelled</c>, recrédite son stock et prévient les admins.</summary>
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly CustomerOrderTransition _transition;

    public CancelOrderCommandHandler(CustomerOrderTransition transition)
    {
        _transition = transition;
    }

    public Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        => _transition.ApplyAsync(
            request.UserId,
            request.Id,
            OrderStatus.Cancelled,
            OrderMessages.CannotBeCancelled,
            EmailTemplates.OrderCancelledByCustomerAdmin,
            cancellationToken);
}
