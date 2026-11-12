using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.Entities;

namespace JewelryShop.API.IntegrationTests.Tags;

// La base est partagée : chaque test crée ses propres tags, aux noms uniques, et ne lit que les articles qui les portent.
[Collection(ApiCollection.Name)]
public class CatalogTagFilterTests
{
    private readonly JewelryShopApiFactory _api;

    public CatalogTagFilterTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Renvoie_les_breloques_qui_portent_un_des_tags_d_une_meme_categorie_et_un_tag_de_chaque_categorie()
    {
        var (sea, beach, golden) = await SeedSeaBeachAndGoldenAsync();
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var seaGolden = await CreateCharmAsync(admin, sea.Id, golden.Id);
        var beachGolden = await CreateCharmAsync(admin, beach.Id, golden.Id);
        var seaOnly = await CreateCharmAsync(admin, sea.Id);
        await CreateCharmAsync(admin, golden.Id);

        var ids = await GetIdsAsync(CharmsApi.Path, $"{sea.Slug},{beach.Slug},{golden.Slug}");

        Assert.Equal([seaGolden, beachGolden], ids);
        Assert.DoesNotContain(seaOnly, ids);
    }

    [Fact]
    public async Task Renvoie_les_chaines_qui_portent_un_des_tags_d_une_meme_categorie()
    {
        var (sea, beach, golden) = await SeedSeaBeachAndGoldenAsync();
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var seaChain = await CreateChainAsync(admin, sea.Id);
        var beachChain = await CreateChainAsync(admin, beach.Id, golden.Id);
        await CreateChainAsync(admin, golden.Id);

        var ids = await GetIdsAsync(ChainsApi.Path, $"{sea.Slug},{beach.Slug}");

        Assert.Equal([seaChain, beachChain], ids);
    }

    [Fact]
    public async Task Combine_les_tags_de_categories_differentes_sur_les_chaines()
    {
        var (sea, beach, golden) = await SeedSeaBeachAndGoldenAsync();
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        await CreateChainAsync(admin, sea.Id);
        var beachGolden = await CreateChainAsync(admin, beach.Id, golden.Id);

        var ids = await GetIdsAsync(ChainsApi.Path, $"{sea.Slug},{beach.Slug},{golden.Slug}");

        Assert.Equal([beachGolden], ids);
    }

    [Fact]
    public async Task Ignore_un_slug_inconnu_et_repond_200()
    {
        var (sea, _, _) = await SeedSeaBeachAndGoldenAsync();
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var seaCharm = await CreateCharmAsync(admin, sea.Id);

        var response = await _api.CreateApiClient().GetAsync($"{CharmsApi.Path}?pageSize=100&tags={sea.Slug},slug-inconnu-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<ListedProduct>>();
        Assert.Equal([seaCharm], page!.Items.Select(c => c.Id));
    }

    [Fact]
    public async Task Ne_filtre_pas_quand_aucun_slug_n_est_connu()
    {
        var anonymous = _api.CreateApiClient();
        var unfiltered = await anonymous.GetFromJsonAsync<PagedResult<ListedProduct>>(CharmsApi.Path);

        var response = await anonymous.GetAsync($"{CharmsApi.Path}?tags=slug-inconnu-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<ListedProduct>>();
        Assert.Equal(unfiltered!.TotalCount, page!.TotalCount);
    }

    [Fact]
    public async Task Compte_et_pagine_seulement_les_articles_filtres()
    {
        var (sea, _, _) = await SeedSeaBeachAndGoldenAsync();
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        await CreateChainAsync(admin, sea.Id);
        var second = await CreateChainAsync(admin, sea.Id);

        var page = await _api.CreateApiClient().GetFromJsonAsync<PagedResult<ListedProduct>>(
            $"{ChainsApi.Path}?tags={sea.Slug}&page=2&pageSize=1");

        Assert.Equal(2, page!.TotalCount);
        Assert.Equal([second], page.Items.Select(c => c.Id));
    }

    [Fact]
    public async Task Renvoie_la_liste_des_chaines_sans_le_parametre_tags_pour_un_appel_anonyme()
    {
        var response = await _api.CreateApiClient().GetAsync(ChainsApi.Path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>Seul l'id est lu : les listes de chaînes et de breloques sont comparées sur leurs ids.</summary>
    private record ListedProduct(long Id);

    private record SeededTag(long Id, string Slug);

    private async Task<(SeededTag Sea, SeededTag Beach, SeededTag Golden)> SeedSeaBeachAndGoldenAsync()
        => (await SeedAsync("Mer", TagCategory.Theme),
            await SeedAsync("Plage", TagCategory.Theme),
            await SeedAsync("Doré", TagCategory.Color));

    private async Task<SeededTag> SeedAsync(string prefix, TagCategory category)
    {
        var name = TagsApi.UniqueName(prefix);
        return new SeededTag(await _api.SeedTagAsync(name, category), TagSlug.From(name));
    }

    private static async Task<long> CreateCharmAsync(HttpClient admin, params long[] tagIds)
        => (await admin.CreateCharmAsync(CharmsApi.Body(CharmsApi.UniqueName(), tagIds))).Id;

    private static async Task<long> CreateChainAsync(HttpClient admin, params long[] tagIds)
        => (await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName(), tagIds: tagIds))).Id;

    private async Task<IReadOnlyList<long>> GetIdsAsync(string path, string tags)
    {
        var page = await _api.CreateApiClient().GetFromJsonAsync<PagedResult<ListedProduct>>($"{path}?pageSize=100&tags={tags}");
        return page!.Items.Select(c => c.Id).ToList();
    }
}
