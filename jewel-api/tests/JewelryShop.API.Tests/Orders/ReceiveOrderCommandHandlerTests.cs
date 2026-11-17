using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Entities;
using static JewelryShop.API.Tests.Orders.CustomerOrderTransitionFixture;

namespace JewelryShop.API.Tests.Orders;

public class ReceiveOrderCommandHandlerTests
{
    private readonly CustomerOrderTransitionFixture _fixture = new();
    private readonly ReceiveOrderCommandHandler _handler;

    public ReceiveOrderCommandHandlerTests()
    {
        _handler = new ReceiveOrderCommandHandler(_fixture.Transition);
    }

    [Fact]
    public async Task Marque_recue_une_commande_envoyee_et_renvoie_la_commande_a_jour()
    {
        var order = _fixture.SeedOrder(OrderStatus.Shipped);

        var dto = await Receive(order.Id);

        Assert.Equal(OrderStatus.Received, order.Status);
        Assert.Equal(Now, order.UpdatedAt);
        Assert.Equal(nameof(OrderStatus.Received), dto.Status);
    }

    [Fact]
    public async Task Ajoute_la_reception_par_le_client_a_l_historique()
    {
        var order = _fixture.SeedOrder(OrderStatus.Shipped);

        await Receive(order.Id);

        var entry = order.StatusHistory.Last();
        Assert.Equal(OrderStatus.Shipped, entry.FromStatus);
        Assert.Equal(OrderStatus.Received, entry.ToStatus);
        Assert.Equal(CustomerId, entry.ChangedBy);
        Assert.False(entry.CustomerNotified);
    }

    [Fact]
    public async Task Ne_touche_pas_au_stock()
    {
        var order = _fixture.SeedOrder(OrderStatus.Shipped);

        await Receive(order.Id);

        Assert.Equal(RollRemainingCm, _fixture.Orders.Rolls[OrderTestData.RollId].RemainingCm);
        Assert.Equal(CloverStock, _fixture.Orders.CharmStocks[OrderTestData.CloverId]);
        Assert.Equal(HeartStock, _fixture.Orders.CharmStocks[OrderTestData.HeartId]);
    }

    [Fact]
    public async Task Previent_chaque_admin()
    {
        var order = _fixture.SeedOrder(OrderStatus.Shipped);

        await Receive(order.Id);

        Assert.Equal(["admin1@mycharmly.eu", "admin2@mycharmly.eu"], _fixture.Outbox.All.Select(m => m.ToAddress));
        Assert.All(_fixture.Outbox.All, m => Assert.Equal($"Commande {order.Number} reçue par Marie Durand", m.Subject));
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Refuse_en_409_une_commande_qui_n_est_pas_envoyee_sans_rien_modifier(OrderStatus status)
    {
        var order = _fixture.SeedOrder(status);

        var error = await Assert.ThrowsAsync<ConflictException>(() => Receive(order.Id));

        Assert.Equal(OrderMessages.CannotBeReceived, error.Message);
        Assert.Equal(status, order.Status);
        Assert.Single(order.StatusHistory);
        Assert.Empty(_fixture.Outbox.All);
    }

    [Fact]
    public async Task Refuse_en_404_la_commande_d_un_autre_utilisateur()
    {
        var order = _fixture.SeedOrder(OrderStatus.Shipped, userId: OtherCustomerId);

        var error = await Assert.ThrowsAsync<NotFoundException>(() => Receive(order.Id));

        Assert.Equal(OrderMessages.NotFound, error.Message);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Empty(_fixture.Outbox.All);
    }

    private Task<OrderDto> Receive(long orderId)
        => _handler.Handle(new ReceiveOrderCommand(CustomerId, orderId), CancellationToken.None);
}
