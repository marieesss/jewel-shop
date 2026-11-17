using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Models;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Orders.Queries;

namespace JewelryShop.API.Tests.Orders;

public class GetMyOrdersQueryHandlerTests
{
    private const long CustomerId = 4;
    private const long OtherCustomerId = 5;

    private static readonly DateTime _start = new(2026, 11, 1, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeOrderRepository _orders = new();
    private readonly GetMyOrdersQueryHandler _handler;

    public GetMyOrdersQueryHandlerTests() => _handler = new GetMyOrdersQueryHandler(_orders);

    private void SeedOrders(long userId, int count, long firstId = 1)
    {
        for (var i = 0; i < count; i++)
            _orders.Seed(OrderTestData.NewOrder(firstId + i, userId, OrderStatus.Pending, _start.AddHours(i)));
    }

    [Fact]
    public async Task Renvoie_les_20_commandes_les_plus_recentes_sur_25()
    {
        SeedOrders(CustomerId, 25);

        var result = await _handler.Handle(new GetMyOrdersQuery(CustomerId), CancellationToken.None);

        Assert.Equal(25, result.TotalCount);
        Assert.Equal(PaginationDefaults.DefaultPageSize, result.PageSize);
        Assert.Equal(Enumerable.Range(6, 20).Select(i => (long)i).Reverse(), result.Items.Select(o => o.Id));
    }

    [Fact]
    public async Task Renvoie_les_5_plus_anciennes_en_page_2()
    {
        SeedOrders(CustomerId, 25);

        var result = await _handler.Handle(new GetMyOrdersQuery(CustomerId, Page: 2), CancellationToken.None);

        Assert.Equal([5L, 4, 3, 2, 1], result.Items.Select(o => o.Id));
        Assert.False(result.HasNext);
    }

    [Fact]
    public async Task Ignore_les_commandes_des_autres_utilisateurs()
    {
        SeedOrders(CustomerId, 2);
        SeedOrders(OtherCustomerId, 3, firstId: 10);

        var result = await _handler.Handle(new GetMyOrdersQuery(CustomerId), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, o => Assert.True(o.Id < 10));
    }

    [Fact]
    public async Task Donne_pour_chaque_commande_son_numero_sa_date_son_statut_et_son_total()
    {
        _orders.Seed(OrderTestData.NewOrder(7, CustomerId, OrderStatus.Shipped, _start));

        var result = await _handler.Handle(new GetMyOrdersQuery(CustomerId), CancellationToken.None);

        var order = Assert.Single(result.Items);
        Assert.Equal((7L, "CMD-2026-0007", _start, "Shipped", 16m), (order.Id, order.Number, order.CreatedAt, order.Status, order.Total));
    }

    [Theory]
    [InlineData(0, PaginationDefaults.DefaultPageSize)]
    [InlineData(500, PaginationDefaults.MaxPageSize)]
    public async Task Borne_la_taille_de_page(int requested, int expected)
    {
        var result = await _handler.Handle(new GetMyOrdersQuery(CustomerId, PageSize: requested), CancellationToken.None);

        Assert.Equal(expected, result.PageSize);
    }
}
