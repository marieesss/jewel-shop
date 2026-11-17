using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Models;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Features.Orders.Queries;

namespace JewelryShop.API.Tests.Orders;

public class GetOrdersQueryHandlerTests
{
    private static readonly DateTime _start = new(2026, 11, 1, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeOrderRepository _orders = new();
    private readonly FakeUserRepository _users = new();
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTests()
    {
        _users.Seed(AdminOrderTestData.Customer(), AdminOrderTestData.OtherCustomer());
        _handler = new GetOrdersQueryHandler(_orders, _users);
    }

    private void SeedOrders(int count, OrderStatus status = OrderStatus.Pending, long firstId = 1)
    {
        for (var i = 0; i < count; i++)
        {
            var id = firstId + i;
            var userId = id % 2 == 0 ? AdminOrderTestData.CustomerId : AdminOrderTestData.OtherCustomerId;
            _orders.Seed(OrderTestData.NewOrder(id, userId, status, _start.AddHours(id)));
        }
    }

    [Fact]
    public async Task Renvoie_les_20_commandes_les_plus_recentes_sur_30_tous_clients_confondus()
    {
        SeedOrders(30);

        var result = await _handler.Handle(new GetOrdersQuery(null), CancellationToken.None);

        Assert.Equal(30, result.TotalCount);
        Assert.Equal(PaginationDefaults.DefaultPageSize, result.PageSize);
        Assert.Equal(Enumerable.Range(11, 20).Select(i => (long)i).Reverse(), result.Items.Select(o => o.Id));
        Assert.True(result.HasNext);
    }

    [Fact]
    public async Task Ne_renvoie_que_les_commandes_du_statut_demande()
    {
        SeedOrders(3, OrderStatus.Pending);
        SeedOrders(2, OrderStatus.Shipped, firstId: 10);

        var result = await _handler.Handle(new GetOrdersQuery("Shipped"), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, o => Assert.Equal("Shipped", o.Status));
    }

    [Fact]
    public async Task Accepte_un_statut_sans_tenir_compte_de_la_casse()
    {
        SeedOrders(1, OrderStatus.Shipped);
        SeedOrders(1, OrderStatus.Pending, firstId: 10);

        var result = await _handler.Handle(new GetOrdersQuery("shipped"), CancellationToken.None);

        Assert.Equal([1L], result.Items.Select(o => o.Id));
    }

    [Fact]
    public async Task Donne_pour_chaque_commande_son_numero_sa_date_son_statut_son_total_et_son_client()
    {
        _orders.Seed(OrderTestData.NewOrder(7, AdminOrderTestData.CustomerId, OrderStatus.Shipped, _start));

        var result = await _handler.Handle(new GetOrdersQuery(null), CancellationToken.None);

        var order = Assert.Single(result.Items);
        Assert.Equal((7L, "CMD-2026-0007", _start, "Shipped", 16m), (order.Id, order.Number, order.CreatedAt, order.Status, order.Total));
        Assert.Equal((AdminOrderTestData.CustomerId, "Marie", "Durand"), (order.Customer.Id, order.Customer.Name, order.Customer.Surname));
    }

    [Theory]
    [InlineData(0, PaginationDefaults.DefaultPageSize)]
    [InlineData(500, PaginationDefaults.MaxPageSize)]
    public async Task Borne_la_taille_de_page(int requested, int expected)
    {
        var result = await _handler.Handle(new GetOrdersQuery(null, PageSize: requested), CancellationToken.None);

        Assert.Equal(expected, result.PageSize);
    }
}
