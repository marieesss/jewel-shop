using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.Entities;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Tags;

public class TagCatalogFilterTests
{
    private readonly FakeTagRepository _tags = new();

    public TagCatalogFilterTests() => _tags.Seed(
        Tag(1, "Mer"),
        Tag(2, "Plage"),
        Tag(3, "Doré", TagCategory.Color));

    [Fact]
    public async Task Combine_les_tags_d_une_categorie_en_ou_et_les_categories_en_et()
    {
        var filter = await _tags.GetCatalogFilterAsync("mer,plage,dore", CancellationToken.None);

        Assert.Equal([[1L, 2L], [3L]], filter.TagIdGroups);
    }

    [Fact]
    public async Task Ignore_un_slug_inconnu()
    {
        var filter = await _tags.GetCatalogFilterAsync("mer,inconnu", CancellationToken.None);

        Assert.Equal([[1L]], filter.TagIdGroups);
    }

    [Fact]
    public async Task Ne_filtre_pas_quand_aucun_slug_n_est_connu()
    {
        var filter = await _tags.GetCatalogFilterAsync("inconnu", CancellationToken.None);

        Assert.True(filter.IsEmpty);
    }

    [Fact]
    public async Task Ne_cherche_aucun_tag_sans_parametre()
    {
        var filter = await _tags.GetCatalogFilterAsync(null, CancellationToken.None);

        Assert.True(filter.IsEmpty);
        Assert.Equal(0, _tags.SlugLookups);
    }
}
