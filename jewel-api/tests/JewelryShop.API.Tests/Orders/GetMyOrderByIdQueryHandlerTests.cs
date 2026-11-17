using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Orders.Queries;

namespace JewelryShop.API.Tests.Orders;

public class GetMyOrderByIdQueryHandlerTests
{
    private const long CustomerId = 4;
    private const long OtherCustomerId = 5;
    private const long OrderId = 42;

    private static readonly DateTime _now = new(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeOrderRepository _orders = new();
    private readonly GetMyOrderByIdQueryHandler _handler;

    public GetMyOrderByIdQueryHandlerTests()
    {
        _orders.Seed(OrderTestData.NewOrder(OrderId, CustomerId, OrderStatus.Pending, _now));
        _handler = new GetMyOrderByIdQueryHandler(_orders);
    }

    [Fact]
    public async Task Renvoie_le_detail_de_la_commande_avec_les_lignes_dans_l_ordre_du_collier()
    {
        var order = await _handler.Handle(new GetMyOrderByIdQuery(CustomerId, OrderId), CancellationToken.None);

        Assert.Equal(OrderId, order.Id);
        Assert.Equal(["Forçat", "Cœur", "Trèfle"], order.Lines.Select(l => l.Name));
        Assert.Equal("Pending", Assert.Single(order.StatusHistory).ToStatus);
    }

    [Fact]
    public async Task Repond_introuvable_pour_la_commande_d_un_autre_utilisateur()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetMyOrderByIdQuery(OtherCustomerId, OrderId), CancellationToken.None));

        Assert.Equal("Commande introuvable.", exception.Message);
    }

    [Fact]
    public async Task Repond_introuvable_pour_une_commande_qui_n_existe_pas()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetMyOrderByIdQuery(CustomerId, 999), CancellationToken.None));

        Assert.Equal("Commande introuvable.", exception.Message);
    }
}
