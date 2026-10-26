using JewelryShop.API.IntegrationTests.Infrastructure;

namespace JewelryShop.API.IntegrationTests.ChainLengths;

[Collection(ApiCollection.Name)]
public class GetChainLengthsTests
{
    private static readonly (int LengthCm, string Label)[] _initialLengths =
    [
        (38, "Ras du cou"),
        (42, "Court"),
        (45, "Princesse"),
        (50, "Mi-long"),
        (55, "Matinée"),
        (60, "Long"),
        (65, "Opéra court"),
        (70, "Opéra"),
        (81, "Sautoir")
    ];

    private readonly JewelryShopApiFactory _api;

    public GetChainLengthsTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Renvoie_sans_connexion_les_9_longueurs_initiales_avec_leur_libelle()
    {
        var anonymous = _api.CreateApiClient();

        var lengths = await anonymous.GetChainLengthsAsync();

        // La base est partagée entre les tests : d'autres longueurs ont pu être ajoutées.
        var initial = lengths.Where(l => _initialLengths.Any(i => i.LengthCm == l.LengthCm));
        Assert.Equal(_initialLengths, initial.Select(l => (l.LengthCm, l.Label)));
    }

    [Fact]
    public async Task Trie_les_longueurs_de_la_plus_courte_a_la_plus_longue()
    {
        var anonymous = _api.CreateApiClient();

        var lengths = await anonymous.GetChainLengthsAsync();

        Assert.Equal(lengths.Select(l => l.LengthCm).Order(), lengths.Select(l => l.LengthCm));
    }
}
