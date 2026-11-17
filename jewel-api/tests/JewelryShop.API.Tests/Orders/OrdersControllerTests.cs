using System.Reflection;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Controllers;
using JewelryShop.Features.Orders.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Tests.Orders;

public class OrdersControllerTests
{
    private const long ConnectedUserId = 4;

    private static readonly DateTime _now = new(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);

    private readonly FakeSender _sender = new();
    private readonly OrdersController _controller = new();

    public OrdersControllerTests() => _sender.AttachTo(_controller, ConnectedUserId);

    private static OrderDto Order() => new(
        42,
        "CMD-2026-0042",
        "Pending",
        null,
        new OrderRecipientDto("Marie", "Durand", "0612345678"),
        new OrderAddressDto("12 rue des Lilas", null, "75011", "Paris"),
        [new OrderLineDto("Chain", 3, "Forçat", 45, null, 10m, 2m, 6.50m)],
        6.50m,
        [new OrderStatusChangeDto(null, "Pending", _now)],
        _now,
        _now);

    [Fact]
    public async Task Commande_pour_l_utilisateur_connecte_et_non_celui_du_corps()
    {
        _sender.Response = Order();
        var body = new PlaceOrderCommand(99, 3, 45, [10], null, null, null, false);

        await _controller.PlaceOrder(body, CancellationToken.None);

        var sent = Assert.IsType<PlaceOrderCommand>(_sender.LastRequest);
        Assert.Equal(body with { UserId = ConnectedUserId }, sent);
    }

    [Fact]
    public async Task Repond_201_avec_l_url_de_la_commande_et_son_detail()
    {
        var order = Order();
        _sender.Response = order;

        var response = await _controller.PlaceOrder(
            new PlaceOrderCommand(0, 3, 45, [10], null, null, null, false), CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(response);
        Assert.Equal("/api/orders/42", created.Location);
        Assert.Same(order, created.Value);
    }

    [Fact]
    public void Est_reserve_aux_utilisateurs_connectes_sous_la_route_api_orders()
    {
        var authorize = typeof(OrdersController).GetCustomAttribute<AuthorizeAttribute>();
        var route = typeof(OrdersController).GetCustomAttribute<RouteAttribute>();
        var post = typeof(OrdersController).GetMethod(nameof(OrdersController.PlaceOrder))!.GetCustomAttribute<HttpPostAttribute>();

        Assert.NotNull(authorize);
        Assert.Null(authorize.Roles);
        Assert.Equal("api/orders", route?.Template);
        Assert.NotNull(post);
        Assert.Null(post.Template);
    }
}
