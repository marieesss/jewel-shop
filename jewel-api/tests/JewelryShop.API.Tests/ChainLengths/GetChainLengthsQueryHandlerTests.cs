using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.ChainLengths.DTOs;
using JewelryShop.Features.ChainLengths.Entities;
using JewelryShop.Features.ChainLengths.Queries;

namespace JewelryShop.API.Tests.ChainLengths;

public class GetChainLengthsQueryHandlerTests
{
    private readonly FakeChainLengthRepository _lengths = new();

    [Fact]
    public async Task Renvoie_les_longueurs_triees_de_la_plus_courte_a_la_plus_longue_avec_leur_libelle()
    {
        _lengths.Seed(
            new ChainLength { Id = 1, LengthCm = 50, Label = "Mi-long" },
            new ChainLength { Id = 2, LengthCm = 38, Label = "Ras du cou" },
            new ChainLength { Id = 3, LengthCm = 45, Label = "Princesse" });
        var handler = new GetChainLengthsQueryHandler(_lengths);

        var result = await handler.Handle(new GetChainLengthsQuery(), CancellationToken.None);

        Assert.Equal(
            [new ChainLengthDto(2, 38, "Ras du cou"), new ChainLengthDto(3, 45, "Princesse"), new ChainLengthDto(1, 50, "Mi-long")],
            result);
    }

    [Fact]
    public async Task Renvoie_une_liste_vide_sans_longueur()
    {
        var handler = new GetChainLengthsQueryHandler(_lengths);

        var result = await handler.Handle(new GetChainLengthsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
