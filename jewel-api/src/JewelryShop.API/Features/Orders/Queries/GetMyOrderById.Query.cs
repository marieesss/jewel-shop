using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

public record GetMyOrderByIdQuery(long UserId, long OrderId) : IRequest<OrderDto>;
