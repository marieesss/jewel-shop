using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Charms.Queries;
using JewelryShop.Features.Tags.Entities;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Charms;

public class GetCharmsQueryHandlerTests
{
    private readonly FakeCharmRepository _charms = new();
    private readonly FakeTagRepository _tags = new();
    private readonly GetCharmsQueryHandler _handler;

    public GetCharmsQueryHandlerTests() => _handler = new GetCharmsQueryHandler(_charms, _tags);

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

    [Fact]
    public async Task Transmet_au_repository_les_tags_demandes_groupes_par_categorie()
    {
        _tags.Seed(Tag(1, "Mer"), Tag(2, "Plage"), Tag(3, "Doré", TagCategory.Color));

        await _handler.Handle(new GetCharmsQuery(null, null, null, null, Tags: "mer,plage,dore"), CancellationToken.None);

        Assert.Equal([[1L, 2L], [3L]], _charms.LastTagFilter!.TagIdGroups);
    }

    [Fact]
    public async Task Ne_filtre_pas_par_tag_sans_le_parametre()
    {
        await _handler.Handle(new GetCharmsQuery(null, null, null, null), CancellationToken.None);

        Assert.True(_charms.LastTagFilter!.IsEmpty);
    }
}
