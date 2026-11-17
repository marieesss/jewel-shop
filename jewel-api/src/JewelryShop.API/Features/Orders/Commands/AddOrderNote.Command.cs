using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Commands;

/// <summary>Note interne ajoutée par l'admin connecté (<see cref="AuthorId"/>) sur une commande.</summary>
public record AddOrderNoteCommand(long OrderId, long AuthorId, string Content) : IRequest<OrderNoteDto>;
