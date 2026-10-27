using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Tags.Entities;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Chains;

public class CreateChainCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 19, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeChainRepository _chains = new();
    private readonly FakeTagRepository _tags = new();
    private readonly CreateChainCommandHandler _handler;

    public CreateChainCommandHandlerTests() =>
        _handler = new CreateChainCommandHandler(_chains, new FakeChainLengthRepository(), _tags, new FakeTimeProvider(_now));

    private static CreateChainCommand LegacyCommand(decimal length = 300m) =>
        new("Gourmette", "Maille fine", ProductColor.Gold, Cost: 12m, Price: 30m, Length: length);

    private static CreateChainCommand MeterCommand() =>
        new("Gourmette", "Maille fine", ProductColor.Gold,
            CostPerMeter: 4m, PricePerMeter: 10m, ClaspCost: 1m, ClaspPrice: 3m, InitialRollLengthCm: 500);

    [Fact]
    public async Task Calcule_les_prix_au_metre_et_cree_un_rouleau_a_l_ancien_format()
    {
        var dto = await _handler.Handle(LegacyCommand(), CancellationToken.None);

        var chain = Assert.Single(_chains.All);
        Assert.Equal(12m, chain.Cost);
        Assert.Equal(30m, chain.Price);
        Assert.Equal(300m, chain.Length);
        Assert.Equal(4m, chain.CostPerMeter);
        Assert.Equal(10m, chain.PricePerMeter);
        var roll = Assert.Single(chain.Rolls);
        Assert.Equal(300, roll.InitialLengthCm);
        Assert.Equal(300, roll.RemainingLengthCm);
        Assert.Equal(_now.UtcDateTime, roll.CreatedAt);
        Assert.Equal(300, dto.StockCm);
        Assert.Equal(1, _chains.SaveChangesCount);
    }

    [Fact]
    public async Task Met_le_fermoir_a_zero_a_l_ancien_format()
    {
        var dto = await _handler.Handle(LegacyCommand(), CancellationToken.None);

        Assert.Equal(0m, dto.ClaspCost);
        Assert.Equal(0m, dto.ClaspPrice);
    }

    [Fact]
    public async Task Ne_cree_pas_de_rouleau_quand_l_ancienne_longueur_arrondie_vaut_zero()
    {
        await _handler.Handle(LegacyCommand(length: 0.4m), CancellationToken.None);

        Assert.Empty(Assert.Single(_chains.All).Rolls);
    }

    [Fact]
    public async Task Enregistre_le_nouveau_format_sans_remplir_les_anciennes_colonnes()
    {
        var dto = await _handler.Handle(MeterCommand(), CancellationToken.None);

        var chain = Assert.Single(_chains.All);
        Assert.Null(chain.Cost);
        Assert.Null(chain.Price);
        Assert.Null(chain.Length);
        Assert.Equal(4m, chain.CostPerMeter);
        Assert.Equal(10m, chain.PricePerMeter);
        Assert.Equal(1m, chain.ClaspCost);
        Assert.Equal(3m, chain.ClaspPrice);
        Assert.Equal(500, Assert.Single(chain.Rolls).InitialLengthCm);
        Assert.Equal(500, dto.StockCm);
    }

    [Fact]
    public async Task Fait_primer_le_nouveau_format_quand_les_deux_sont_envoyes()
    {
        var command = MeterCommand() with { Cost = 12m, Price = 30m, Length = 300m };

        await _handler.Handle(command, CancellationToken.None);

        var chain = Assert.Single(_chains.All);
        Assert.Null(chain.Price);
        Assert.Equal(10m, chain.PricePerMeter);
        Assert.Equal(500, Assert.Single(chain.Rolls).InitialLengthCm);
    }

    [Fact]
    public async Task Associe_les_tags_une_seule_fois_tries_par_categorie_puis_nom()
    {
        _tags.Seed(Tag(1, "Mer"), Tag(2, "Or", TagCategory.Color), Tag(3, "Bohème"));

        var dto = await _handler.Handle(MeterCommand() with { TagIds = [2, 1, 3, 1] }, CancellationToken.None);

        var chain = Assert.Single(_chains.All);
        Assert.Equal([1L, 2L, 3L], chain.ChainTags.Select(ct => ct.TagId).Order());
        Assert.Equal(["Bohème", "Mer", "Or"], dto.Tags!.Select(t => t.Name));
        Assert.Equal(["Theme", "Theme", "Color"], dto.Tags!.Select(t => t.Category));
    }

    [Fact]
    public async Task Cree_la_chaine_sans_tag_quand_tag_ids_est_absent()
    {
        var dto = await _handler.Handle(MeterCommand(), CancellationToken.None);

        Assert.Empty(Assert.Single(_chains.All).ChainTags);
        Assert.Empty(dto.Tags!);
    }

    [Fact]
    public async Task Refuse_un_tag_inconnu_sans_creer_la_chaine()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(MeterCommand() with { TagIds = [42] }, CancellationToken.None));

        Assert.Equal(["Tag introuvable."], exception.Errors[nameof(CreateChainCommand.TagIds)]);
        Assert.Empty(_chains.All);
    }
}
