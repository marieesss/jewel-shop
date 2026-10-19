using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Entities;

namespace JewelryShop.API.Tests.Chains;

public class UpdateChainCommandHandlerTests
{
    private const long ChainId = 7;
    private static readonly DateTime _rollDate = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeChainRepository _chains = new();
    private readonly UpdateChainCommandHandler _handler;
    private readonly Chain _chain;

    public UpdateChainCommandHandlerTests()
    {
        _chain = new Chain
        {
            Id = ChainId,
            Name = "Gourmette",
            Color = ProductColor.Gold,
            Cost = 20m,
            Price = 50m,
            Length = 500m,
            CostPerMeter = 4m,
            PricePerMeter = 10m,
            ClaspCost = 1m,
            ClaspPrice = 3m
        };
        _chain.AddRoll(500, _rollDate);
        _chain.AddRoll(200, _rollDate);
        _chains.Seed(_chain);
        _handler = new UpdateChainCommandHandler(_chains);
    }

    private static UpdateChainCommand LegacyCommand() =>
        new(ChainId, "Gourmette", "Maille fine", ProductColor.Gold, Cost: 15m, Price: 60m, Length: 300m);

    [Fact]
    public async Task Garde_les_rouleaux_et_le_fermoir_a_l_ancien_format()
    {
        var dto = await _handler.Handle(LegacyCommand(), CancellationToken.None);

        Assert.Equal([500, 200], _chain.Rolls.Select(r => r.RemainingLengthCm));
        Assert.Equal(700, dto.StockCm);
        Assert.Equal(1m, _chain.ClaspCost);
        Assert.Equal(3m, _chain.ClaspPrice);
    }

    [Fact]
    public async Task Recalcule_les_prix_au_metre_a_l_ancien_format()
    {
        await _handler.Handle(LegacyCommand(), CancellationToken.None);

        Assert.Equal(15m, _chain.Cost);
        Assert.Equal(60m, _chain.Price);
        Assert.Equal(300m, _chain.Length);
        Assert.Equal(5m, _chain.CostPerMeter);
        Assert.Equal(20m, _chain.PricePerMeter);
        Assert.Equal(1, _chains.SaveChangesCount);
    }

    [Fact]
    public async Task Garde_les_anciennes_colonnes_au_nouveau_format()
    {
        var command = new UpdateChainCommand(
            ChainId, "Gourmette", "Maille fine", ProductColor.Gold,
            CostPerMeter: 6m, PricePerMeter: 14m, ClaspCost: 2m, ClaspPrice: 5m);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(20m, _chain.Cost);
        Assert.Equal(50m, _chain.Price);
        Assert.Equal(500m, _chain.Length);
        Assert.Equal(6m, _chain.CostPerMeter);
        Assert.Equal(14m, _chain.PricePerMeter);
        Assert.Equal(2m, _chain.ClaspCost);
        Assert.Equal(5m, _chain.ClaspPrice);
        Assert.Equal(2, _chain.Rolls.Count);
    }

    [Fact]
    public async Task Refuse_une_chaine_inconnue()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(LegacyCommand() with { Id = 999 }, CancellationToken.None));
    }
}
