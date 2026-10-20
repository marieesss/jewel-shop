using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Chains.Queries;

namespace JewelryShop.API.Tests.Chains;

public class GetChainsQueryHandlerTests
{
    private readonly FakeChainRepository _chains = new();
    private readonly GetChainsQueryHandler _handler;

    public GetChainsQueryHandlerTests() => _handler = new GetChainsQueryHandler(_chains);

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
}
