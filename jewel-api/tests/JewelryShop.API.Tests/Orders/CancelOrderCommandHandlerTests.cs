using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Commands;
using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using JewelryShop.Features.Orders.Entities;
using JewelryShop.Infrastructure.Email;
using static JewelryShop.API.Tests.Orders.CustomerOrderTransitionFixture;

namespace JewelryShop.API.Tests.Orders;

public class CancelOrderCommandHandlerTests
{
    private readonly CustomerOrderTransitionFixture _fixture = new();
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _handler = new CancelOrderCommandHandler(_fixture.Transition);
    }

    [Fact]
    public async Task Annule_une_commande_en_validation_et_renvoie_la_commande_a_jour()
    {
        var order = _fixture.SeedOrder(OrderStatus.Pending);

        var dto = await Cancel(order.Id);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(Now, order.UpdatedAt);
        Assert.Equal(nameof(OrderStatus.Cancelled), dto.Status);
        Assert.Equal(order.Id, dto.Id);
        Assert.Equal(1, _fixture.Orders.TransactionCount);
    }

    [Fact]
    public async Task Recredite_le_rouleau_de_la_chaine_et_le_stock_des_breloques()
    {
        var order = _fixture.SeedOrder(OrderStatus.Pending);

        await Cancel(order.Id);

        Assert.Equal(RollRemainingCm + OrderTestData.LengthCm, _fixture.Orders.Rolls[OrderTestData.RollId].RemainingCm);
        Assert.Equal(CloverStock + 1, _fixture.Orders.CharmStocks[OrderTestData.CloverId]);
        Assert.Equal(HeartStock + 1, _fixture.Orders.CharmStocks[OrderTestData.HeartId]);
    }

    [Fact]
    public async Task Recredite_chaque_exemplaire_d_une_breloque_presente_plusieurs_fois()
    {
        var order = _fixture.SeedOrder(OrderStatus.Pending);
        order.Lines.Add(new OrderLine
        {
            Kind = OrderLineKind.Charm,
            CharmId = OrderTestData.CloverId,
            Name = "Trèfle",
            UnitPrice = 5m,
            Position = 3
        });

        await Cancel(order.Id);

        Assert.Equal(CloverStock + 2, _fixture.Orders.CharmStocks[OrderTestData.CloverId]);
    }

    [Fact]
    public async Task Ne_recredite_aucun_rouleau_pour_une_chaine_sans_rouleau()
    {
        var order = _fixture.SeedOrder(OrderStatus.Pending);
        order.Lines.Single(l => l.Kind == OrderLineKind.Chain).ChainRollId = null;

        await Cancel(order.Id);

        Assert.Equal(RollRemainingCm, _fixture.Orders.Rolls[OrderTestData.RollId].RemainingCm);
        Assert.Equal(HeartStock + 1, _fixture.Orders.CharmStocks[OrderTestData.HeartId]);
    }

    [Fact]
    public async Task Ajoute_l_annulation_par_le_client_a_l_historique()
    {
        var order = _fixture.SeedOrder(OrderStatus.Pending);

        var dto = await Cancel(order.Id);

        var entry = order.StatusHistory.Last();
        Assert.Equal(OrderStatus.Pending, entry.FromStatus);
        Assert.Equal(OrderStatus.Cancelled, entry.ToStatus);
        Assert.Equal(CustomerId, entry.ChangedBy);
        Assert.Null(entry.Message);
        Assert.False(entry.CustomerNotified);
        Assert.Equal(Now, entry.CreatedAt);
        Assert.Equal(2, dto.StatusHistory.Count);
    }

    [Fact]
    public async Task Previent_chaque_admin_sans_ecrire_au_client()
    {
        var order = _fixture.SeedOrder(OrderStatus.Pending);

        await Cancel(order.Id);

        Assert.Equal(["admin1@mycharmly.eu", "admin2@mycharmly.eu"], _fixture.Outbox.All.Select(m => m.ToAddress));
        Assert.All(_fixture.Outbox.All, m => Assert.Equal($"Commande {order.Number} annulée par Marie Durand", m.Subject));
    }

    [Theory]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Refuse_en_409_une_commande_qui_n_est_plus_en_validation_sans_rien_modifier(OrderStatus status)
    {
        var order = _fixture.SeedOrder(status);

        var error = await Assert.ThrowsAsync<ConflictException>(() => Cancel(order.Id));

        Assert.Equal(OrderMessages.CannotBeCancelled, error.Message);
        Assert.Equal(status, order.Status);
        Assert.Single(order.StatusHistory);
        Assert.Equal(RollRemainingCm, _fixture.Orders.Rolls[OrderTestData.RollId].RemainingCm);
        Assert.Equal(CloverStock, _fixture.Orders.CharmStocks[OrderTestData.CloverId]);
        Assert.Empty(_fixture.Outbox.All);
    }

    [Fact]
    public async Task Refuse_en_409_une_commande_annulee_entre_la_lecture_et_la_mise_a_jour()
    {
        var order = _fixture.SeedOrder(OrderStatus.Pending);
        await Cancel(order.Id);

        // Le statut de départ lu est Pending, mais l'UPDATE conditionnel ne trouve plus la commande dans ce statut.
        var transition = _fixture.Transition;
        var error = await Assert.ThrowsAsync<ConflictException>(() => transition.ApplyAsync(
            CustomerId, order.Id, OrderStatus.Cancelled, OrderMessages.CannotBeCancelled,
            EmailTemplates.OrderCancelledByCustomerAdmin, CancellationToken.None));

        Assert.Equal(OrderMessages.CannotBeCancelled, error.Message);
        Assert.Equal(RollRemainingCm + OrderTestData.LengthCm, _fixture.Orders.Rolls[OrderTestData.RollId].RemainingCm);
    }

    [Fact]
    public async Task Refuse_en_404_la_commande_d_un_autre_utilisateur_sans_rien_modifier()
    {
        var order = _fixture.SeedOrder(OrderStatus.Pending, userId: OtherCustomerId);

        var error = await Assert.ThrowsAsync<NotFoundException>(() => Cancel(order.Id));

        Assert.Equal(OrderMessages.NotFound, error.Message);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(CloverStock, _fixture.Orders.CharmStocks[OrderTestData.CloverId]);
        Assert.Empty(_fixture.Outbox.All);
    }

    [Fact]
    public async Task Refuse_en_404_une_commande_inconnue()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => Cancel(999));

        Assert.Equal(OrderMessages.NotFound, error.Message);
    }

    private Task<OrderDto> Cancel(long orderId)
        => _handler.Handle(new CancelOrderCommand(CustomerId, orderId), CancellationToken.None);
}
