using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Users.Common;
using MediatR;

namespace JewelryShop.Features.Orders.Commands;

public class AddOrderNoteCommandHandler : IRequestHandler<AddOrderNoteCommand, OrderNoteDto>
{
    private readonly IOrderRepository _orders;
    private readonly IUserRepository _users;
    private readonly TimeProvider _timeProvider;

    public AddOrderNoteCommandHandler(IOrderRepository orders, IUserRepository users, TimeProvider timeProvider)
    {
        _orders = orders;
        _users = users;
        _timeProvider = timeProvider;
    }

    public async Task<OrderNoteDto> Handle(AddOrderNoteCommand request, CancellationToken cancellationToken)
    {
        if (!await _orders.ExistsAsync(request.OrderId, cancellationToken))
            throw new NotFoundException(OrderMessages.NotFound);

        var author = await _users.GetByIdAsNoTrackingAsync(request.AuthorId, cancellationToken)
            ?? throw new NotFoundException(UserMessages.EntityName, request.AuthorId);

        var now = OrderNoteClock.UtcNow(_timeProvider);
        var note = new OrderNote
        {
            OrderId = request.OrderId,
            AuthorId = author.Id,
            Content = request.Content.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _orders.AddNoteAsync(note, cancellationToken);
        await _orders.SaveChangesAsync(cancellationToken);

        return note.ToOrderNoteDto(author);
    }
}
