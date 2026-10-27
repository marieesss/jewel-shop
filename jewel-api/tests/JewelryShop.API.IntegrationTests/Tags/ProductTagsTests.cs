using System.Net;
using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Chains;
using JewelryShop.API.IntegrationTests.Charms;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Common;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.DTOs;
using JewelryShop.Features.Charms.DTOs;
using JewelryShop.Features.Tags.Entities;

namespace JewelryShop.API.IntegrationTests.Tags;

[Collection(ApiCollection.Name)]
public class ProductTagsTests
{
    private readonly JewelryShopApiFactory _api;

    public ProductTagsTests(JewelryShopApiFactory api) => _api = api;

    [Fact]
    public async Task Associe_les_tags_envoyes_a_une_chaine()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var theme = await _api.SeedTagAsync(TagsApi.UniqueName());
        var color = await _api.SeedTagAsync(TagsApi.UniqueName(), TagCategory.Color);
        var created = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));

        var response = await admin.PutAsJsonAsync($"{ChainsApi.Path}/{created.Id}", ChainsApi.MeterBody(created.Name, tagIds: [color, theme]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<AdminChainDto>();
        Assert.Equal([theme, color], updated!.Tags!.Select(t => t.Id));
        var detail = await _api.CreateApiClient().GetFromJsonAsync<ChainDto>($"{ChainsApi.Path}/{created.Id}");
        Assert.Equal([theme, color], detail!.Tags!.Select(t => t.Id));
    }

    [Fact]
    public async Task Garde_les_tags_d_une_chaine_modifiee_sans_tag_ids()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var tag = await _api.SeedTagAsync(TagsApi.UniqueName());
        var created = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName(), tagIds: [tag]));

        var response = await admin.PutAsJsonAsync($"{ChainsApi.Path}/{created.Id}", ChainsApi.LegacyBody(created.Name));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<AdminChainDto>();
        Assert.Equal([tag], updated!.Tags!.Select(t => t.Id));
        var detail = await admin.GetFromJsonAsync<AdminChainDto>($"{ChainsApi.Path}/{created.Id}");
        Assert.Equal([tag], detail!.Tags!.Select(t => t.Id));
    }

    [Fact]
    public async Task Retire_les_tags_d_une_breloque_avec_une_liste_vide()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var tag = await _api.SeedTagAsync(TagsApi.UniqueName());
        var created = await admin.CreateCharmAsync(CharmsApi.Body(CharmsApi.UniqueName(), [tag]));
        Assert.Equal([tag], created.Tags!.Select(t => t.Id));

        var response = await admin.PutAsJsonAsync($"{CharmsApi.Path}/{created.Id}", CharmsApi.Body(created.Name, []));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<AdminCharmDto>();
        Assert.Empty(updated!.Tags!);
        var detail = await admin.GetFromJsonAsync<AdminCharmDto>($"{CharmsApi.Path}/{created.Id}");
        Assert.Empty(detail!.Tags!);
    }

    [Fact]
    public async Task Refuse_un_tag_inconnu()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var created = await admin.CreateChainAsync(ChainsApi.MeterBody(ChainsApi.UniqueName()));

        var response = await admin.PutAsJsonAsync($"{ChainsApi.Path}/{created.Id}", ChainsApi.MeterBody(created.Name, tagIds: [long.MaxValue]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.Equal(["Tag introuvable."], error!.Errors!["TagIds"]);
    }

    [Fact]
    public async Task Renvoie_les_tags_de_chaque_breloque_a_un_visiteur()
    {
        var admin = await _api.CreateClientAsAsync(UserRole.Admin);
        var color = await _api.SeedTagAsync(TagsApi.UniqueName(), TagCategory.Color);
        var theme = await _api.SeedTagAsync(TagsApi.UniqueName());
        // Prix propre à ce test, pour retrouver la breloque quelle que soit la taille du catalogue.
        const decimal price = 913.17m;
        var body = new
        {
            Name = CharmsApi.UniqueName(),
            Description = "Argent massif",
            Color = ProductColor.Silver,
            Cost = 2m,
            Price = price,
            Stock = 10,
            TagIds = new[] { color, theme }
        };
        var created = await admin.CreateCharmAsync(body);

        var page = await _api.CreateApiClient().GetFromJsonAsync<PagedResult<CharmDto>>(
            $"{CharmsApi.Path}?minPrice=913.17&maxPrice=913.17&pageSize=100");

        var charm = Assert.Single(page!.Items, c => c.Id == created.Id);
        Assert.Equal([theme, color], charm.Tags!.Select(t => t.Id));
        Assert.Equal(["Theme", "Color"], charm.Tags!.Select(t => t.Category));
    }
}
