using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Infrastructure.Email;
using MediatR;

namespace JewelryShop.Features.Orders.Commands;

/// <summary>Passe la commande <c>Shipped</c> en <c>Received</c> et prévient les admins.</summary>
public class ReceiveOrderCommandHandler : IRequestHandler<ReceiveOrderCommand, OrderDto>
{
    private readonly CustomerOrderTransition _transition;

    public ReceiveOrderCommandHandler(CustomerOrderTransition transition)
    {
        _transition = transition;
    }

    public Task<OrderDto> Handle(ReceiveOrderCommand request, CancellationToken cancellationToken)
        => _transition.ApplyAsync(
            request.UserId,
            request.Id,
            OrderStatus.Received,
            OrderMessages.CannotBeReceived,
            EmailTemplates.OrderReceivedAdmin,
            cancellationToken);
}
