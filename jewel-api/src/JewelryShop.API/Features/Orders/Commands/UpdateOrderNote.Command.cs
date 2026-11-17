using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Commands;

/// <summary>Remplace le contenu d'une note interne. L'auteur reste celui qui l'a écrite.</summary>
public record UpdateOrderNoteCommand(long OrderId, long NoteId, string Content) : IRequest<OrderNoteDto>;
