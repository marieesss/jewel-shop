using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.ChainLengths.Entities;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Charms.Entities;
using JewelryShop.Features.Orders.Common;

namespace JewelryShop.API.Tests.Orders;

public class NecklaceComposerTests
{
    private const long ChainId = 3;
    private const long CloverId = 10;
    private const long HeartId = 11;

    private sealed record Composition(long ChainId, int LengthCm, IReadOnlyList<long> CharmIds) : INecklaceComposition;

    private readonly FakeChainRepository _chains = new();
    private readonly FakeChainLengthRepository _lengths = new();
    private readonly FakeCharmRepository _charms = new();
    private readonly NecklaceComposer _composer;

    public NecklaceComposerTests()
    {
        _composer = new NecklaceComposer(_chains, _lengths, _charms);
        _lengths.Seed(new ChainLength { Id = 1, LengthCm = 45, Label = "Princesse" }, new ChainLength { Id = 2, LengthCm = 60, Label = "Long" });
        _charms.Seed(Charm(CloverId, "Trèfle", stock: 2), Charm(HeartId, "Cœur", stock: 1));
    }

    private static Chain Chain(int rollLengthCm = 50, decimal? pricePerMeter = 10m)
    {
        var chain = new Chain
        {
            Id = ChainId,
            Name = "Forçat",
            Color = ProductColor.Gold,
            PricePerMeter = pricePerMeter,
            ClaspPrice = 2m,
        };
        chain.AddRoll(rollLengthCm, new DateTime(2026, 11, 2, 8, 0, 0, DateTimeKind.Utc));
        return chain;
    }

    private static Charm Charm(long id, string name, int stock) =>
        new() { Id = id, Name = name, Color = ProductColor.Silver, Price = 5m, Stock = stock };

    private Task<Necklace> ComposeAsync(int lengthCm, params long[] charmIds) =>
        _composer.ComposeAsync(new Composition(ChainId, lengthCm, charmIds), CancellationToken.None);

    [Fact]
    public async Task Renvoie_la_chaine_et_les_breloques_dans_l_ordre_demande()
    {
        _chains.Seed(Chain());

        var necklace = await ComposeAsync(45, HeartId, CloverId, CloverId);

        Assert.Equal(ChainId, necklace.Chain.Id);
        Assert.Equal(45, necklace.LengthCm);
        Assert.Equal([HeartId, CloverId, CloverId], necklace.Charms.Select(c => c.Id));
    }

    [Fact]
    public async Task Refuse_une_chaine_inexistante_avec_404()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => ComposeAsync(45, CloverId));

        Assert.Equal("Chaîne introuvable.", error.Message);
    }

    [Fact]
    public async Task Refuse_une_longueur_absente_des_longueurs_proposees()
    {
        _chains.Seed(Chain());

        var error = await Assert.ThrowsAsync<ValidationException>(() => ComposeAsync(46, CloverId));

        Assert.Equal(["Longueur non proposée."], error.Errors["LengthCm"]);
    }

    [Fact]
    public async Task Refuse_une_breloque_inexistante_avec_404()
    {
        _chains.Seed(Chain());

        var error = await Assert.ThrowsAsync<NotFoundException>(() => ComposeAsync(45, CloverId, 99));

        Assert.Equal("Breloque introuvable.", error.Message);
    }

    [Fact]
    public async Task Refuse_une_longueur_qu_aucun_rouleau_ne_couvre_avec_409()
    {
        _chains.Seed(Chain(rollLengthCm: 50));

        var error = await Assert.ThrowsAsync<ConflictException>(() => ComposeAsync(60, CloverId));

        Assert.Equal("Le stock de « Forçat » ne permet plus cette commande.", error.Message);
    }

    [Fact]
    public async Task Accepte_une_longueur_egale_au_plus_long_rouleau()
    {
        _chains.Seed(Chain(rollLengthCm: 45));

        var necklace = await ComposeAsync(45, CloverId);

        Assert.Equal(45, necklace.LengthCm);
    }

    [Fact]
    public async Task Refuse_une_chaine_sans_prix_au_metre_avec_409()
    {
        _chains.Seed(Chain(pricePerMeter: null));

        var error = await Assert.ThrowsAsync<ConflictException>(() => ComposeAsync(45, CloverId));

        Assert.Equal("Le stock de « Forçat » ne permet plus cette commande.", error.Message);
    }

    [Fact]
    public async Task Refuse_plus_d_exemplaires_d_une_breloque_que_son_stock_avec_409()
    {
        _chains.Seed(Chain());

        var error = await Assert.ThrowsAsync<ConflictException>(() => ComposeAsync(45, CloverId, HeartId, CloverId, CloverId));

        Assert.Equal("Le stock de « Trèfle » ne permet plus cette commande.", error.Message);
    }

    [Fact]
    public async Task Accepte_autant_d_exemplaires_d_une_breloque_que_son_stock()
    {
        _chains.Seed(Chain());

        var necklace = await ComposeAsync(45, CloverId, CloverId, HeartId);

        Assert.Equal(3, necklace.Charms.Count);
    }

    [Fact]
    public async Task N_enregistre_rien()
    {
        _chains.Seed(Chain());

        await ComposeAsync(45, CloverId);

        Assert.Equal(0, _chains.SaveChangesCount + _charms.SaveChangesCount + _lengths.SaveChangesCount);
    }
}
