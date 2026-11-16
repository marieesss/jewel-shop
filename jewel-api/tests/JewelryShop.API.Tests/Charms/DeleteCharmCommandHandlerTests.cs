using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Charms.Commands;
using JewelryShop.Features.Charms.Entities;
using JewelryShop.Features.Orders.Entities;

namespace JewelryShop.API.Tests.Charms;

public class DeleteCharmCommandHandlerTests
{
    private const long CharmId = 5;
    private const long UnknownCharmId = 99;

    private readonly FakeCharmRepository _charms = new();
    private readonly FakeOrderRepository _orders = new();
    private readonly DeleteCharmCommandHandler _handler;

    public DeleteCharmCommandHandlerTests()
    {
        _charms.Seed(new Charm { Id = CharmId, Name = "Trèfle" });
        _handler = new DeleteCharmCommandHandler(_charms, _orders);
    }

    [Fact]
    public async Task Supprime_une_breloque_absente_des_commandes()
    {
        await _handler.Handle(new DeleteCharmCommand(CharmId), CancellationToken.None);

        Assert.Empty(_charms.All);
        Assert.Equal(1, _charms.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_supprimer_une_breloque_presente_dans_une_commande()
    {
        _orders.Seed(new Order
        {
            Lines = { new OrderLine { Kind = OrderLineKind.Charm, CharmId = CharmId, Name = "Trèfle", Position = 1 } }
        });

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new DeleteCharmCommand(CharmId), CancellationToken.None));

        Assert.Equal("Ce produit figure dans une commande.", exception.Message);
        Assert.Single(_charms.All);
        Assert.Equal(0, _charms.SaveChangesCount);
    }

    [Fact]
    public async Task Supprime_une_breloque_quand_seule_une_chaine_de_meme_id_est_commandee()
    {
        _orders.Seed(new Order { Lines = { new OrderLine { Kind = OrderLineKind.Chain, ChainId = CharmId, Name = "Forçat" } } });

        await _handler.Handle(new DeleteCharmCommand(CharmId), CancellationToken.None);

        Assert.Empty(_charms.All);
    }

    [Fact]
    public async Task Signale_une_breloque_inexistante()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new DeleteCharmCommand(UnknownCharmId), CancellationToken.None));
    }
}
