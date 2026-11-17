using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Commands;

/// <summary>Annulation par le client de sa commande, possible tant qu'elle est en validation.</summary>
public record CancelOrderCommand(long UserId, long Id) : IRequest<OrderDto>;
