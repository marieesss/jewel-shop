using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Entities;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public async Task Cree_un_tag_avec_son_slug_et_le_rend_visible_publiquement()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var response = await admin.PostAsJsonAsync(TagsApi.AdminPath, new { Name = $" Mer {suffix} ", Category = "Theme" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(created);
        Assert.Equal($"Mer {suffix}", created.Name);
        Assert.Equal($"mer-{suffix}", created.Slug);
        Assert.Equal("Theme", created.Category);
        Assert.Contains(await _api.CreateApiClient().GetTagsAsync(), t => t.Id == created.Id);
    }

    [Fact]
    public async Task Refuse_un_nom_deja_pris_dans_la_categorie_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var name = TagsApi.UniqueName("Mer");
        await _api.SeedTagAsync(name, TagCategory.Theme);

        var response = await admin.PostAsJsonAsync(TagsApi.AdminPath, new { Name = name.ToUpperInvariant(), Category = "Theme" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal($"Le tag « {name.ToUpperInvariant()} » existe déjà dans cette catégorie.", error?.Title);
    }

    [Fact]
    public async Task Refuse_un_nom_qui_donne_le_slug_d_un_tag_de_la_categorie_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await _api.SeedTagAsync($"Doré {suffix}", TagCategory.Color);

        var response = await admin.PostAsJsonAsync(TagsApi.AdminPath, new { Name = $"Dore {suffix}", Category = "Color" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal($"Un tag au nom trop proche existe déjà dans cette catégorie : « Doré {suffix} ».", error?.Title);
    }

    [Fact]
    public async Task Accepte_le_meme_nom_dans_deux_categories()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var name = $"Doré {Guid.NewGuid().ToString("N")[..8]}";

        var color = await admin.PostAsJsonAsync(TagsApi.AdminPath, new { Name = name, Category = "Color" });
        var material = await admin.PostAsJsonAsync(TagsApi.AdminPath, new { Name = name, Category = "Material" });

        Assert.Equal(HttpStatusCode.Created, color.StatusCode);
        Assert.Equal(HttpStatusCode.Created, material.StatusCode);
        var tags = (await _api.CreateApiClient().GetTagsAsync()).Where(t => t.Name == name).ToList();
        Assert.Equal(["Color", "Material"], tags.Select(t => t.Category));
    }

    [Theory]
    [InlineData("Mer", "Saison", "Category", "Catégorie invalide.")]
    [InlineData("Mer", "0", "Category", "Catégorie invalide.")]
    [InlineData("", "Theme", "Name", "Le nom est requis.")]
    [InlineData("!!!", "Theme", "Name", "Le nom doit contenir au moins une lettre ou un chiffre.")]
    public async Task Refuse_un_tag_invalide_avec_400(string name, string category, string field, string message)
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync(TagsApi.AdminPath, new { Name = name, Category = category });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(error?.Errors);
        Assert.Contains(message, Assert.Contains(field, error.Errors));
    }

    [Fact]
    public async Task Refuse_la_creation_a_un_utilisateur_non_admin_avec_403()
    {
        var user = await _api.CreateClientAsAsync(UserRole.User);

        var response = await user.PostAsJsonAsync(TagsApi.AdminPath, new { Name = TagsApi.UniqueName(), Category = "Theme" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Refuse_la_creation_sans_connexion_avec_401()
    {
        var response = await _api.CreateApiClient().PostAsJsonAsync(TagsApi.AdminPath, new { Name = TagsApi.UniqueName(), Category = "Theme" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Renomme_un_tag_et_recalcule_son_slug()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var tagId = await _api.SeedTagAsync($"Ocean {suffix}", TagCategory.Theme);

        var response = await admin.PutAsJsonAsync($"{TagsApi.AdminPath}/{tagId}", new { Name = $"Océan Indien {suffix}" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var renamed = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.Equal($"Océan Indien {suffix}", renamed?.Name);
        Assert.Equal($"ocean-indien-{suffix}", renamed?.Slug);
        Assert.Equal("Theme", renamed?.Category);
    }

    [Fact]
    public async Task Refuse_de_renommer_vers_un_nom_deja_pris_avec_409()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var taken = TagsApi.UniqueName("Plage");
        await _api.SeedTagAsync(taken, TagCategory.Theme);
        var tagId = await _api.SeedTagAsync(TagsApi.UniqueName("Mer"), TagCategory.Theme);

        var response = await admin.PutAsJsonAsync($"{TagsApi.AdminPath}/{tagId}", new { Name = taken });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Refuse_de_renommer_un_tag_inconnu_avec_404()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.PutAsJsonAsync($"{TagsApi.AdminPath}/{long.MaxValue}", new { Name = "Inconnu" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Supprime_un_tag_et_ses_associations_sans_supprimer_les_articles()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var tagId = await _api.SeedTagAsync(TagsApi.UniqueName());
        var chainId = (await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()))).Id;
        var charmId = await admin.CreateCharmAsync();
        await _api.TagProductsAsync(tagId, [chainId], [charmId]);

        var response = await admin.DeleteAsync($"{TagsApi.AdminPath}/{tagId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var (tagExists, links, chainExists, charmExists) = await _api.QueryAsync(async services =>
        {
            var context = services.GetRequiredService<AppDbContext>();
            return (
                await context.Tags.AnyAsync(t => t.Id == tagId),
                await context.ChainTags.CountAsync(l => l.TagId == tagId) + await context.CharmTags.CountAsync(l => l.TagId == tagId),
                await context.Chains.AnyAsync(c => c.Id == chainId),
                await context.Charms.AnyAsync(c => c.Id == charmId));
        });
        Assert.False(tagExists);
        Assert.Equal(0, links);
        Assert.True(chainExists);
        Assert.True(charmExists);
    }

    [Fact]
    public async Task Refuse_de_supprimer_un_tag_inconnu_avec_404()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);

        var response = await admin.DeleteAsync($"{TagsApi.AdminPath}/{long.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Refuse_la_suppression_a_un_utilisateur_non_admin_avec_403()
    {
        var user = await _api.CreateClientAsAsync(UserRole.User);
        var tagId = await _api.SeedTagAsync(TagsApi.UniqueName());

        var response = await user.DeleteAsync($"{TagsApi.AdminPath}/{tagId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
