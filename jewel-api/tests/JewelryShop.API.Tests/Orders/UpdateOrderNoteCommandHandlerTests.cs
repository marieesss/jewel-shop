using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Entities;

namespace JewelryShop.API.Tests.Orders;

public class UpdateOrderNoteCommandHandlerTests
{
    private const long OrderId = 42;
    private const long OtherOrderId = 43;
    private const long NoteId = 7;

    private static readonly DateTime _createdAt = new(2026, 11, 16, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _now = new(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeOrderRepository _orders = new();
    private readonly FakeUserRepository _users = new();
    private readonly UpdateOrderNoteCommandHandler _handler;
    private readonly OrderNote _note;

    public UpdateOrderNoteCommandHandlerTests()
    {
        _users.Seed(AdminOrderTestData.Customer(), AdminOrderTestData.Admin(), AdminOrderTestData.OtherAdmin());
        var order = OrderTestData.NewOrder(OrderId, AdminOrderTestData.CustomerId, OrderStatus.Pending, _createdAt);
        _note = new OrderNote
        {
            Id = NoteId,
            OrderId = OrderId,
            AuthorId = AdminOrderTestData.AdminId,
            Content = "Emballage cadeau.",
            CreatedAt = _createdAt,
            UpdatedAt = _createdAt
        };
        order.Notes.Add(_note);
        _orders.Seed(order, OrderTestData.NewOrder(OtherOrderId, AdminOrderTestData.CustomerId, OrderStatus.Pending, _createdAt));
        _handler = new UpdateOrderNoteCommandHandler(_orders, _users, new FakeTimeProvider(new DateTimeOffset(_now)));
    }

    [Fact]
    public async Task Remplace_le_contenu_et_la_date_de_modification_sans_changer_l_auteur()
    {
        var note = await _handler.Handle(
            new UpdateOrderNoteCommand(OrderId, NoteId, " Emballage cadeau, ruban rouge. "), CancellationToken.None);

        Assert.Equal("Emballage cadeau, ruban rouge.", note.Content);
        Assert.Equal((_createdAt, _now), (note.CreatedAt, note.UpdatedAt));
        Assert.Equal(AdminOrderTestData.AdminId, note.Author.Id);
        Assert.Equal(("Emballage cadeau, ruban rouge.", _now), (_note.Content, _note.UpdatedAt));
    }

    [Fact]
    public async Task Repond_introuvable_pour_une_note_inconnue()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new UpdateOrderNoteCommand(OrderId, 999, "Note"), CancellationToken.None));

        Assert.Equal("Note introuvable.", exception.Message);
    }

    [Fact]
    public async Task Repond_introuvable_pour_la_note_d_une_autre_commande()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new UpdateOrderNoteCommand(OtherOrderId, NoteId, "Note"), CancellationToken.None));

        Assert.Equal("Note introuvable.", exception.Message);
        Assert.Equal("Emballage cadeau.", _note.Content);
    }

    [Fact]
    public async Task Repond_introuvable_pour_une_commande_inconnue()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new UpdateOrderNoteCommand(999, NoteId, "Note"), CancellationToken.None));

        Assert.Equal("Commande introuvable.", exception.Message);
    }
}
