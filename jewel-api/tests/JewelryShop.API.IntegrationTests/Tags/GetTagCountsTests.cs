using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Tags.DTOs;

namespace JewelryShop.API.IntegrationTests.Tags;

// La base est partagée : chaque test crée son propre tag, au nom unique, et ne lit que son nombre d'articles.
[Collection(ApiCollection.Name)]
public class GetTagCountsTests
{
    private readonly JewelryShopApiFactory _api;

    public GetTagCountsTests(JewelryShopApiFactory api) => _api = api;

    [Theory]
    [InlineData("charm", 10)]
    [InlineData("chain", 2)]
    public async Task Compte_les_articles_du_type_demande_qui_portent_le_tag(string kind, int expectedCount)
    {
        var sea = await SeedTagOnTenCharmsAndTwoChainsAsync();

        var tags = await GetTagCountsAsync(kind);

        Assert.Equal(expectedCount, Assert.Single(tags, t => t.Id == sea).Count);
    }

    [Fact]
    public async Task Compte_zero_pour_un_tag_sans_article()
    {
        var unused = await _api.SeedTagAsync(TagsApi.UniqueName());

        var tags = await GetTagCountsAsync("charm");

        Assert.Equal(0, Assert.Single(tags, t => t.Id == unused).Count);
    }

    [Fact]
    public async Task Refuse_un_type_d_article_inconnu()
    {
        var response = await _api.CreateApiClient().GetAsync($"{TagsApi.PublicPath}?kind=creation");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(["Type d'article invalide."], error!.Errors!["Kind"]);
    }

    [Fact]
    public async Task Ne_renvoie_pas_de_nombre_sans_type_d_article()
    {
        var id = await _api.SeedTagAsync(TagsApi.UniqueName());

        var tags = await _api.CreateApiClient().GetFromJsonAsync<List<JsonElement>>(TagsApi.PublicPath);

        var tag = Assert.Single(tags!, t => t.GetProperty("id").GetInt64() == id);
        Assert.False(tag.TryGetProperty("count", out _));
    }

    private async Task<long> SeedTagOnTenCharmsAndTwoChainsAsync()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var tag = await _api.SeedTagAsync(TagsApi.UniqueName("Mer"));
        var charmIds = new List<long>();
        for (var i = 0; i < 10; i++)
            charmIds.Add(await admin.CreateCharmAsync());
        var chainIds = new List<long>();
        for (var i = 0; i < 2; i++)
            chainIds.Add((await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()))).Id);
        await _api.TagProductsAsync(tag, chainIds, charmIds);
        return tag;
    }

    private async Task<List<TagCountDto>> GetTagCountsAsync(string kind)
    {
        var response = await _api.CreateApiClient().GetAsync($"{TagsApi.PublicPath}?kind={kind}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<TagCountDto>>())!;
    }
}
