using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

/// <summary>Détail d'une commande de n'importe quel client, pour l'admin.</summary>
public record GetOrderByIdQuery(long OrderId) : IRequest<AdminOrderDto>;
