using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Charms.Queries;

namespace JewelryShop.API.Tests.Charms;

public class GetCharmsQueryHandlerTests
{
    private readonly FakeCharmRepository _charms = new();
    private readonly GetCharmsQueryHandler _handler;

    public GetCharmsQueryHandlerTests() => _handler = new GetCharmsQueryHandler(_charms);

    [Fact]
    public async Task Transmet_au_repository_l_utilisateur_dont_les_favoris_passent_en_premier()
    {
        await _handler.Handle(
            new GetCharmsQuery(null, null, null, null, FavoritesFirstForUserId: 42), CancellationToken.None);

        Assert.Equal(42, _charms.LastFavoritesFirstForUserId);
    }

    [Fact]
    public async Task Ne_demande_pas_les_favoris_en_premier_sans_utilisateur()
    {
        await _handler.Handle(new GetCharmsQuery(null, null, null, null), CancellationToken.None);

        Assert.Null(_charms.LastFavoritesFirstForUserId);
    }
}
