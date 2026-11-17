using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Orders.Queries;

namespace JewelryShop.API.Tests.Orders;

public class GetOrderByIdQueryHandlerTests
{
    private const long OrderId = 42;

    private static readonly DateTime _now = new(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeOrderRepository _orders = new();
    private readonly FakeUserRepository _users = new();
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _users.Seed(AdminOrderTestData.Customer(), AdminOrderTestData.Admin(), AdminOrderTestData.OtherAdmin());
        var order = OrderTestData.NewOrder(OrderId, AdminOrderTestData.CustomerId, OrderStatus.Processing, _now);
        order.StatusHistory.Add(new OrderStatusHistory
        {
            Id = 1000,
            FromStatus = OrderStatus.Pending,
            ToStatus = OrderStatus.Processing,
            ChangedBy = AdminOrderTestData.AdminId,
            Message = "Collier en préparation.",
            CustomerNotified = true,
            CreatedAt = _now.AddHours(1)
        });
        order.Notes.Add(new OrderNote
        {
            Id = 2,
            OrderId = OrderId,
            AuthorId = AdminOrderTestData.OtherAdminId,
            Content = "Rappeler la cliente.",
            CreatedAt = _now.AddHours(3),
            UpdatedAt = _now.AddHours(3)
        });
        order.Notes.Add(new OrderNote
        {
            Id = 1,
            OrderId = OrderId,
            AuthorId = AdminOrderTestData.AdminId,
            Content = "Emballage cadeau.",
            CreatedAt = _now.AddHours(2),
            UpdatedAt = _now.AddHours(2)
        });
        _orders.Seed(order);
        _handler = new GetOrderByIdQueryHandler(_orders, _users);
    }

    [Fact]
    public async Task Renvoie_le_detail_avec_le_client_et_son_email()
    {
        var order = await _handler.Handle(new GetOrderByIdQuery(OrderId), CancellationToken.None);

        Assert.Equal(OrderId, order.Id);
        Assert.Equal(
            (AdminOrderTestData.CustomerId, "Marie", "Durand", "marie@example.com"),
            (order.Customer.Id, order.Customer.Name, order.Customer.Surname, order.Customer.Email));
        Assert.Equal(["Forçat", "Cœur", "Trèfle"], order.Lines.Select(l => l.Name));
    }

    [Fact]
    public async Task Renvoie_l_historique_complet_avec_l_auteur_le_message_et_l_envoi_au_client()
    {
        var order = await _handler.Handle(new GetOrderByIdQuery(OrderId), CancellationToken.None);

        Assert.Equal(2, order.StatusHistory.Count);
        Assert.Equal(AdminOrderTestData.CustomerId, order.StatusHistory[0].Author.Id);
        var change = order.StatusHistory[1];
        Assert.Equal(("Pending", "Processing"), (change.FromStatus, change.ToStatus));
        Assert.Equal(("Alice", "Martin"), (change.Author.Name, change.Author.Surname));
        Assert.Equal("Collier en préparation.", change.Message);
        Assert.True(change.CustomerNotified);
    }

    [Fact]
    public async Task Renvoie_les_notes_de_la_plus_ancienne_a_la_plus_recente_avec_leur_auteur()
    {
        var order = await _handler.Handle(new GetOrderByIdQuery(OrderId), CancellationToken.None);

        Assert.Equal(["Emballage cadeau.", "Rappeler la cliente."], order.Notes.Select(n => n.Content));
        Assert.Equal([AdminOrderTestData.AdminId, AdminOrderTestData.OtherAdminId], order.Notes.Select(n => n.Author.Id));
        Assert.Equal(_now.AddHours(2), order.Notes[0].CreatedAt);
    }

    [Fact]
    public async Task Repond_introuvable_pour_une_commande_inconnue()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetOrderByIdQuery(999), CancellationToken.None));

        Assert.Equal("Commande introuvable.", exception.Message);
    }
}
