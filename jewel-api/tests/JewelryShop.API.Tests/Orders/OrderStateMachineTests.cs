using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.Entities;

namespace JewelryShop.API.Tests.Orders;

public class OrderStateMachineTests
{
    private static readonly (OrderStatus From, OrderStatus To)[] _customerTransitions =
    [
        (OrderStatus.Pending, OrderStatus.Cancelled),
        (OrderStatus.Shipped, OrderStatus.Received)
    ];

    private static readonly (OrderStatus From, OrderStatus To)[] _adminTransitions =
    [
        (OrderStatus.Pending, OrderStatus.Processing),
        (OrderStatus.Pending, OrderStatus.Shipped),
        (OrderStatus.Pending, OrderStatus.Received),
        (OrderStatus.Pending, OrderStatus.Cancelled),
        (OrderStatus.Processing, OrderStatus.Pending),
        (OrderStatus.Processing, OrderStatus.Shipped),
        (OrderStatus.Processing, OrderStatus.Received),
        (OrderStatus.Processing, OrderStatus.Cancelled),
        (OrderStatus.Shipped, OrderStatus.Pending),
        (OrderStatus.Shipped, OrderStatus.Processing),
        (OrderStatus.Shipped, OrderStatus.Received),
        (OrderStatus.Shipped, OrderStatus.Cancelled),
        (OrderStatus.Received, OrderStatus.Pending),
        (OrderStatus.Received, OrderStatus.Processing),
        (OrderStatus.Received, OrderStatus.Shipped)
    ];

    public static TheoryData<OrderStatus, OrderStatus> AllowedForCustomer() => ToTheoryData(_customerTransitions);

    public static TheoryData<OrderStatus, OrderStatus> RefusedForCustomer() => ToTheoryData(AllPairs().Except(_customerTransitions));

    public static TheoryData<OrderStatus, OrderStatus> AllowedForAdmin() => ToTheoryData(_adminTransitions);

    public static TheoryData<OrderStatus, OrderStatus> RefusedForAdmin() => ToTheoryData(AllPairs().Except(_adminTransitions));

    [Theory]
    [MemberData(nameof(AllowedForCustomer))]
    public void Accepte_l_annulation_en_validation_et_la_reception_d_un_envoi_par_le_client(OrderStatus from, OrderStatus to)
    {
        Assert.True(OrderStateMachine.CanTransition(from, to, OrderActor.Customer));
    }

    [Theory]
    [MemberData(nameof(RefusedForCustomer))]
    public void Refuse_toute_autre_transition_demandee_par_le_client(OrderStatus from, OrderStatus to)
    {
        Assert.False(OrderStateMachine.CanTransition(from, to, OrderActor.Customer));
    }

    [Fact]
    public void Refuse_au_client_l_annulation_d_une_commande_en_traitement()
    {
        Assert.False(OrderStateMachine.CanTransition(OrderStatus.Processing, OrderStatus.Cancelled, OrderActor.Customer));
    }

    [Theory]
    [MemberData(nameof(AllowedForAdmin))]
    public void Accepte_les_transitions_de_l_admin_entre_statuts_actifs_et_l_annulation_avant_reception(OrderStatus from, OrderStatus to)
    {
        Assert.True(OrderStateMachine.CanTransition(from, to, OrderActor.Admin));
    }

    [Theory]
    [MemberData(nameof(RefusedForAdmin))]
    public void Refuse_a_l_admin_les_transitions_depuis_annulee_l_annulation_d_une_commande_recue_et_le_meme_statut(
        OrderStatus from, OrderStatus to)
    {
        Assert.False(OrderStateMachine.CanTransition(from, to, OrderActor.Admin));
    }

    [Fact]
    public void Accepte_que_l_admin_repasse_une_commande_recue_en_traitement()
    {
        Assert.True(OrderStateMachine.CanTransition(OrderStatus.Received, OrderStatus.Processing, OrderActor.Admin));
    }

    [Theory]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending)]
    [InlineData(OrderStatus.Received, OrderStatus.Cancelled)]
    public void Refuse_a_l_admin_de_reprendre_une_commande_annulee_ou_d_annuler_une_commande_recue(OrderStatus from, OrderStatus to)
    {
        Assert.False(OrderStateMachine.CanTransition(from, to, OrderActor.Admin));
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Processing)]
    public void Recredite_le_stock_a_l_annulation_d_une_commande_pas_encore_envoyee(OrderStatus from)
    {
        Assert.True(OrderStateMachine.RestocksOnCancellation(from));
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Received)]
    [InlineData(OrderStatus.Cancelled)]
    public void Ne_recredite_pas_le_stock_d_une_commande_envoyee_recue_ou_deja_annulee(OrderStatus from)
    {
        Assert.False(OrderStateMachine.RestocksOnCancellation(from));
    }

    private static IEnumerable<(OrderStatus From, OrderStatus To)> AllPairs() =>
        from source in Enum.GetValues<OrderStatus>()
        from target in Enum.GetValues<OrderStatus>()
        select (source, target);

    private static TheoryData<OrderStatus, OrderStatus> ToTheoryData(IEnumerable<(OrderStatus From, OrderStatus To)> pairs)
    {
        var data = new TheoryData<OrderStatus, OrderStatus>();
        foreach (var (from, to) in pairs)
            data.Add(from, to);
        return data;
    }
}
