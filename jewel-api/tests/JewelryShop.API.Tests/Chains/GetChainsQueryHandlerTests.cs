using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Features.ChainLengths.Entities;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Chains.Queries;

namespace JewelryShop.API.Tests.Chains;

public class GetChainsQueryHandlerTests
{
    private readonly FakeChainRepository _chains = new();
    private readonly FakeChainLengthRepository _lengths = new();
    private readonly GetChainsQueryHandler _handler;

    public GetChainsQueryHandlerTests() => _handler = new GetChainsQueryHandler(_chains, _lengths);

    [Fact]
    public async Task Transmet_au_repository_l_utilisateur_dont_les_favoris_passent_en_premier()
    {
        await _handler.Handle(
            new GetChainsQuery(null, null, null, FavoritesFirstForUserId: 42), CancellationToken.None);

        Assert.Equal(42, _chains.LastFavoritesFirstForUserId);
    }

    [Fact]
    public async Task Ne_demande_pas_les_favoris_en_premier_sans_utilisateur()
    {
        await _handler.Handle(new GetChainsQuery(null, null, null), CancellationToken.None);

        Assert.Null(_chains.LastFavoritesFirstForUserId);
    }

    [Fact]
    public async Task Renvoie_le_prix_et_la_disponibilite_des_longueurs_de_chaque_chaine()
    {
        var chain = new Chain { Id = 1, Name = "Forçat", Color = ProductColor.Gold, PricePerMeter = 10m, ClaspPrice = 2m };
        chain.AddRoll(50, new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        _chains.Seed(chain);
        _lengths.Seed(
            new ChainLength { Id = 2, LengthCm = 55, Label = "Matinée" },
            new ChainLength { Id = 1, LengthCm = 45, Label = "Princesse" });

        var page = await _handler.Handle(new GetChainsQuery(null, null, null), CancellationToken.None);

        var dto = Assert.Single(page.Items);
        Assert.Equal(
            [new(45, "Princesse", 6.50m, true), new(55, "Matinée", 7.50m, false)],
            dto.AvailableLengths!);
        Assert.Equal(6.50m, dto.FromPrice);
    }
}
