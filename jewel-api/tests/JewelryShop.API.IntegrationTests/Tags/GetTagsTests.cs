using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Features.Tags.Entities;

namespace JewelryShop.API.IntegrationTests.Tags;

// La base est partagée entre les tests : chaque test crée ses propres tags, aux noms uniques.
[Collection(ApiCollection.Name)]
public class GetTagsTests
{
    private readonly JewelryShopApiFactory _api;

    public GetTagsTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Renvoie_sans_connexion_les_tags_avec_leur_slug_et_leur_categorie()
    {
        var name = TagsApi.UniqueName("Mer");
        var id = await _api.SeedTagAsync(name, TagCategory.Theme);

        var tags = await _api.CreateApiClient().GetTagsAsync();

        var tag = Assert.Single(tags, t => t.Id == id);
        Assert.Equal(name, tag.Name);
        Assert.Equal(name.ToLowerInvariant().Replace(' ', '-'), tag.Slug);
        Assert.Equal("Theme", tag.Category);
    }

    [Fact]
    public async Task Trie_les_tags_par_categorie_dans_l_ordre_theme_couleur_matiere_occasion_puis_par_nom()
    {
        var prefix = Guid.NewGuid().ToString("N")[..8];
        var occasion = await _api.SeedTagAsync($"{prefix} a", TagCategory.Occasion);
        var materialB = await _api.SeedTagAsync($"{prefix} b", TagCategory.Material);
        var materialA = await _api.SeedTagAsync($"{prefix} a", TagCategory.Material);
        var color = await _api.SeedTagAsync($"{prefix} a", TagCategory.Color);
        var theme = await _api.SeedTagAsync($"{prefix} z", TagCategory.Theme);

        var tags = await _api.CreateApiClient().GetTagsAsync();

        var ours = tags.Where(t => t.Name.StartsWith(prefix)).Select(t => t.Id);
        Assert.Equal([theme, color, materialA, materialB, occasion], ours);
    }
}
