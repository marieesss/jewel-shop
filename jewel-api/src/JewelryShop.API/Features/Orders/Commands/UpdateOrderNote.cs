using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Users.Common;
using MediatR;

namespace JewelryShop.Features.Orders.Commands;

public class UpdateOrderNoteCommandHandler : IRequestHandler<UpdateOrderNoteCommand, OrderNoteDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;
    private readonly TimeProvider _timeProvider;

    public UpdateOrderNoteCommandHandler(IOrderRepository orders, IUserRepository users, TimeProvider timeProvider)
    {
        _orders = orders;
        _users = users;
        _timeProvider = timeProvider;
    }

    public async Task<OrderNoteDto> Handle(UpdateOrderNoteCommand request, CancellationToken cancellationToken)
    {
        if (!await _orders.ExistsAsync(request.OrderId, cancellationToken))
            throw new NotFoundException(OrderMessages.NotFound);

        // Une note d'une autre commande est introuvable, comme une note inexistante.
        var note = await _orders.GetNoteAsync(request.OrderId, request.NoteId, cancellationToken)
            ?? throw new NotFoundException(OrderMessages.NoteNotFound);

        note.Content = request.Content.Trim();
        note.UpdatedAt = OrderNoteClock.UtcNow(_timeProvider);
        await _orders.SaveChangesAsync(cancellationToken);

        var author = await _users.GetByIdAsNoTrackingAsync(note.AuthorId, cancellationToken)
            ?? throw new NotFoundException(UserMessages.EntityName, note.AuthorId);

        return note.ToOrderNoteDto(author);
    }
}
