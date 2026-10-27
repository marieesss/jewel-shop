using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.IntegrationTests.Tags;

// La base est partagée entre les tests : chaque test crée ses propres tags et articles.
[Collection(ApiCollection.Name)]
public class MergeTagTests
{
    private readonly JewelryShopApiFactory _api;

    public MergeTagTests(JewelryShopApiFactory api) => _api = api;

    private static string MergePath(long id) => $"{TagsApi.AdminPath}/{id}/merge";

    private Task<bool> TagExistsAsync(long tagId)
        => _api.QueryAsync(services => services.GetRequiredService<AppDbContext>().Tags.AnyAsync(t => t.Id == tagId));

    private Task<List<long>> ChainIdsOfAsync(long tagId)
        => _api.QueryAsync(services => services.GetRequiredService<AppDbContext>().ChainTags
            .Where(l => l.TagId == tagId).Select(l => l.ChainId).OrderBy(id => id).ToListAsync());

    private Task<List<long>> TagIdsOfCharmAsync(long charmId)
        => _api.QueryAsync(services => services.GetRequiredService<AppDbContext>().CharmTags
            .Where(l => l.CharmId == charmId).Select(l => l.TagId).ToListAsync());

    private static async Task<long> CreateChainAsync(HttpClient admin)
        => (await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()))).Id;

    [Fact]
    public async Task Reporte_les_chaines_du_tag_fusionne_sur_le_tag_cible_et_supprime_le_tag_fusionne()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var beachId = await _api.SeedTagAsync(TagsApi.UniqueName("Plage"));
        var seaId = await _api.SeedTagAsync(TagsApi.UniqueName("Mer"));
        long[] beachChains = [await CreateChainAsync(admin), await CreateChainAsync(admin)];
        var seaChain = await CreateChainAsync(admin);
        await _api.TagProductsAsync(beachId, beachChains);
        await _api.TagProductsAsync(seaId, [seaChain]);

        var response = await admin.PostAsJsonAsync(MergePath(beachId), new { TargetTagId = seaId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var target = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.Equal(seaId, target?.Id);
        Assert.Equal(new[] { beachChains[0], beachChains[1], seaChain }, await ChainIdsOfAsync(seaId));
        Assert.False(await TagExistsAsync(beachId));
    }

    [Fact]
    public async Task Garde_une_seule_fois_le_tag_cible_sur_un_article_qui_porte_les_deux_tags()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var beachId = await _api.SeedTagAsync(TagsApi.UniqueName("Plage"));
        var seaId = await _api.SeedTagAsync(TagsApi.UniqueName("Mer"));
        var charmId = await admin.CreateCharmAsync();
        var chainId = await CreateChainAsync(admin);
        await _api.TagProductsAsync(beachId, [chainId], [charmId]);
        await _api.TagProductsAsync(seaId, [chainId], [charmId]);

        var response = await admin.PostAsJsonAsync(MergePath(beachId), new { TargetTagId = seaId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] { seaId }, await TagIdsOfCharmAsync(charmId));
        Assert.Equal(new[] { chainId }, await ChainIdsOfAsync(seaId));
    }

    [Fact]
    public async Task Refuse_un_tag_cible_inconnu_avec_404_sans_rien_changer()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var beachId = await _api.SeedTagAsync(TagsApi.UniqueName("Plage"));
        var chainId = await CreateChainAsync(admin);
        await _api.TagProductsAsync(beachId, [chainId]);

        var response = await admin.PostAsJsonAsync(MergePath(beachId), new { TargetTagId = long.MaxValue });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(await TagExistsAsync(beachId));
        Assert.Equal(new[] { chainId }, await ChainIdsOfAsync(beachId));
    }

    [Fact]
    public async Task Refuse_un_tag_source_inconnu_avec_404()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var seaId = await _api.SeedTagAsync(TagsApi.UniqueName("Mer"));

        var response = await admin.PostAsJsonAsync(MergePath(long.MaxValue), new { TargetTagId = seaId });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(await TagExistsAsync(seaId));
    }

    [Fact]
    public async Task Refuse_de_fusionner_un_tag_avec_lui_meme_avec_400()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var tagId = await _api.SeedTagAsync(TagsApi.UniqueName());

        var response = await admin.PostAsJsonAsync(MergePath(tagId), new { TargetTagId = tagId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        Assert.Contains(TagMessages.CannotMergeWithItself, Assert.Contains("TargetTagId", error.Errors));
        Assert.True(await TagExistsAsync(tagId));
    }

    [Fact]
    public async Task Refuse_la_fusion_a_un_utilisateur_non_admin_avec_403()
    {
        var user = await _api.CreateClientAsAsync(UserRole.User);
        var beachId = await _api.SeedTagAsync(TagsApi.UniqueName("Plage"));
        var seaId = await _api.SeedTagAsync(TagsApi.UniqueName("Mer"));

        var response = await user.PostAsJsonAsync(MergePath(beachId), new { TargetTagId = seaId });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.True(await TagExistsAsync(beachId));
    }

    [Fact]
    public async Task Refuse_la_fusion_sans_connexion_avec_401()
    {
        var response = await _api.CreateApiClient().PostAsJsonAsync(MergePath(1), new { TargetTagId = 2 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
