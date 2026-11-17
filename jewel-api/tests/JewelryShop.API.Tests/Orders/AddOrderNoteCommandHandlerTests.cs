using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Entities;

namespace JewelryShop.API.Tests.Orders;

public class AddOrderNoteCommandHandlerTests
{
    private const long OrderId = 42;

    private static readonly DateTime _now = new(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeOrderRepository _orders = new();
    private readonly FakeUserRepository _users = new();
    private readonly AddOrderNoteCommandHandler _handler;

    public AddOrderNoteCommandHandlerTests()
    {
        _users.Seed(AdminOrderTestData.Customer(), AdminOrderTestData.Admin());
        _orders.Seed(OrderTestData.NewOrder(OrderId, AdminOrderTestData.CustomerId, OrderStatus.Pending, _now.AddDays(-1)));
        _handler = new AddOrderNoteCommandHandler(_orders, _users, new FakeTimeProvider(new DateTimeOffset(_now)));
    }

    [Fact]
    public async Task Ajoute_la_note_signee_par_l_admin_connecte_a_la_date_du_jour()
    {
        var note = await _handler.Handle(
            new AddOrderNoteCommand(OrderId, AdminOrderTestData.AdminId, "  Emballage cadeau.  "), CancellationToken.None);

        Assert.True(note.Id > 0);
        Assert.Equal("Emballage cadeau.", note.Content);
        Assert.Equal((AdminOrderTestData.AdminId, "Alice", "Martin"), (note.Author.Id, note.Author.Name, note.Author.Surname));
        Assert.Equal((_now, _now), (note.CreatedAt, note.UpdatedAt));

        var order = await _orders.GetByIdAsync(OrderId);
        var stored = Assert.Single(order!.Notes);
        Assert.Equal((note.Id, AdminOrderTestData.AdminId, "Emballage cadeau."), (stored.Id, stored.AuthorId, stored.Content));
    }

    [Fact]
    public async Task Arrondit_les_dates_a_la_microseconde_comme_la_base()
    {
        var handler = new AddOrderNoteCommandHandler(
            _orders, _users, new FakeTimeProvider(new DateTimeOffset(_now.AddTicks(1234567))));

        var note = await handler.Handle(
            new AddOrderNoteCommand(OrderId, AdminOrderTestData.AdminId, "Note"), CancellationToken.None);

        Assert.Equal(_now.AddTicks(1234560), note.CreatedAt);
    }

    [Fact]
    public async Task Repond_introuvable_pour_une_commande_inconnue()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new AddOrderNoteCommand(999, AdminOrderTestData.AdminId, "Note"), CancellationToken.None));

        Assert.Equal("Commande introuvable.", exception.Message);
    }
}
