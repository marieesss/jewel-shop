using System.Net;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Tags.Entities;

namespace JewelryShop.API.IntegrationTests.Tags;

// La base est partagée entre les tests : chaque test crée ses propres tags, aux noms uniques.
[Collection(ApiCollection.Name)]
public class AdminTagsTests
{
    private readonly JewelryShopApiFactory _api;

    public AdminTagsTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Compte_les_chaines_et_les_breloques_qui_portent_un_tag()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var tagId = await _api.SeedTagAsync(TagsApi.UniqueName());
        var chainIds = new List<long>();
        for (var i = 0; i < 3; i++)
            chainIds.Add((await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()))).Id);
        var charmId = await admin.CreateCharmAsync();
        await _api.TagProductsAsync(tagId, chainIds, [charmId]);

        var tags = await admin.GetAdminTagsAsync();

        var tag = Assert.Single(tags, t => t.Id == tagId);
        Assert.Equal(3, tag.ChainCount);
        Assert.Equal(1, tag.CharmCount);
    }

    [Fact]
    public async Task Renvoie_zero_pour_un_tag_porte_par_aucun_article()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var tagId = await _api.SeedTagAsync(TagsApi.UniqueName(), TagCategory.Occasion);

        var tag = Assert.Single(await admin.GetAdminTagsAsync(), t => t.Id == tagId);

        Assert.Equal(0, tag.ChainCount);
        Assert.Equal(0, tag.CharmCount);
        Assert.Equal("Occasion", tag.Category);
    }

    [Fact]
    public async Task Refuse_la_liste_admin_sans_connexion_avec_401()
    {
        var response = await _api.CreateApiClient().GetAsync(TagsApi.AdminPath);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refuse_la_liste_admin_a_un_utilisateur_non_admin_avec_403()
    {
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.GetAsync(TagsApi.AdminPath);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
