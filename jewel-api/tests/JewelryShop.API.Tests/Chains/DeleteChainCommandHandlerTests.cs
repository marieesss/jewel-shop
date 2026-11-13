using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Orders.Entities;

namespace JewelryShop.API.Tests.Chains;

public class DeleteChainCommandHandlerTests
{
    private const long ChainId = 3;
    private const long UnknownChainId = 99;

    private readonly FakeChainRepository _chains = new();
    private readonly FakeOrderRepository _orders = new();
    private readonly DeleteChainCommandHandler _handler;

    public DeleteChainCommandHandlerTests()
    {
        _chains.Seed(new Chain { Id = ChainId, Name = "Forçat" });
        _handler = new DeleteChainCommandHandler(_chains, _orders);
    }

    [Fact]
    public async Task Supprime_une_chaine_absente_des_commandes()
    {
        await _handler.Handle(new DeleteChainCommand(ChainId), CancellationToken.None);

        Assert.Empty(_chains.All);
        Assert.Equal(1, _chains.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_supprimer_une_chaine_presente_dans_une_commande()
    {
        _orders.Seed(new Order { Lines = { new OrderLine { Kind = OrderLineKind.Chain, ChainId = ChainId, Name = "Forçat" } } });

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new DeleteChainCommand(ChainId), CancellationToken.None));

        Assert.Equal("Ce produit figure dans une commande.", exception.Message);
        Assert.Single(_chains.All);
        Assert.Equal(0, _chains.SaveChangesCount);
    }

    [Fact]
    public async Task Supprime_une_chaine_quand_seule_une_breloque_de_meme_id_est_commandee()
    {
        _orders.Seed(new Order { Lines = { new OrderLine { Kind = OrderLineKind.Charm, CharmId = ChainId, Name = "Trèfle" } } });

        await _handler.Handle(new DeleteChainCommand(ChainId), CancellationToken.None);

        Assert.Empty(_chains.All);
    }

    [Fact]
    public async Task Signale_une_chaine_inexistante()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new DeleteChainCommand(UnknownChainId), CancellationToken.None));
    }
}
