using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Commands;

/// <summary>Confirmation par le client qu'il a reçu sa commande envoyée.</summary>
public record ReceiveOrderCommand(long UserId, long Id) : IRequest<OrderDto>;
