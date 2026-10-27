using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Tags.Entities;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Chains;

public class UpdateChainCommandHandlerTests
{
    private const long ChainId = 7;
    private static readonly DateTime _rollDate = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeChainRepository _chains = new();
    private readonly FakeTagRepository _tags = new();
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
        _handler = new UpdateChainCommandHandler(_chains, new FakeChainLengthRepository(), _tags);
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

    [Fact]
    public async Task Associe_les_tags_demandes_a_la_chaine()
    {
        _tags.Seed(Tag(1, "Mer"), Tag(2, "Or", TagCategory.Color), Tag(3, "Fête", TagCategory.Occasion));
        _chain.ReplaceTags([_tags.All[2]]);

        var dto = await _handler.Handle(LegacyCommand() with { TagIds = [1, 2] }, CancellationToken.None);

        Assert.Equal([1L, 2L], _chain.ChainTags.Select(ct => ct.TagId).Order());
        Assert.Equal(["Mer", "Or"], dto.Tags!.Select(t => t.Name));
    }

    [Fact]
    public async Task Garde_les_tags_quand_tag_ids_est_absent()
    {
        _tags.Seed(Tag(1, "Mer"));
        _chain.ReplaceTags([_tags.All[0]]);

        await _handler.Handle(LegacyCommand(), CancellationToken.None);

        Assert.Equal([1L], _chain.ChainTags.Select(ct => ct.TagId));
    }

    [Fact]
    public async Task Retire_tous_les_tags_avec_une_liste_vide()
    {
        _tags.Seed(Tag(1, "Mer"));
        _chain.ReplaceTags([_tags.All[0]]);

        var dto = await _handler.Handle(LegacyCommand() with { TagIds = [] }, CancellationToken.None);

        Assert.Empty(_chain.ChainTags);
        Assert.Empty(dto.Tags!);
    }

    [Fact]
    public async Task Refuse_un_tag_inconnu_sans_enregistrer()
    {
        _tags.Seed(Tag(1, "Mer"));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(LegacyCommand() with { TagIds = [1, 99] }, CancellationToken.None));

        Assert.Equal(["Tag introuvable."], exception.Errors[nameof(UpdateChainCommand.TagIds)]);
        Assert.Empty(_chain.ChainTags);
        Assert.Equal(0, _chains.SaveChangesCount);
    }
}
