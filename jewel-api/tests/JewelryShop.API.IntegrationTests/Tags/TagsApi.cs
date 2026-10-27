using System.Net.Http.Json;
using JewelryShop.API.IntegrationTests.Infrastructure;
using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Entities;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.IntegrationTests.Tags;

public static class TagsApi
{
    public const string PublicPath = "/api/tags";
    public const string AdminPath = "/api/admin/tags";

    /// <summary>Nom unique, pour qu'un test ne dépende pas des tags créés par les autres.</summary>
    public static string UniqueName(string prefix = "Tag") => $"{prefix} {Guid.NewGuid():N}"[..30];

    public static async Task<List<TagDto>> GetTagsAsync(this HttpClient client)
    {
        var response = await client.GetAsync(PublicPath);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<TagDto>>())!;
    }

    public static async Task<List<AdminTagDto>> GetAdminTagsAsync(this HttpClient admin)
    {
        var response = await admin.GetAsync(AdminPath);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<AdminTagDto>>())!;
    }

    /// <summary>Enregistre un tag directement en base.</summary>
    public static Task<long> SeedTagAsync(this JewelryShopApiFactory api, string name, TagCategory category = TagCategory.Theme)
        => api.QueryAsync(async services =>
        {
            var context = services.GetRequiredService<AppDbContext>();
            var tag = new Tag(name, category);
            context.Tags.Add(tag);
            await context.SaveChangesAsync();
            return tag.Id;
        });

    /// <summary>
    /// Associe des chaînes et des breloques à un tag directement en base, sans passer par l'API.
    /// </summary>
    public static Task TagProductsAsync(this JewelryShopApiFactory api, long tagId, IEnumerable<long> chainIds, IEnumerable<long>? charmIds = null)
        => api.SeedAsync(async services =>
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.ChainTags.AddRange(chainIds.Select(id => new ChainTag { ChainId = id, TagId = tagId }));
            context.CharmTags.AddRange((charmIds ?? []).Select(id => new CharmTag { CharmId = id, TagId = tagId }));
            await context.SaveChangesAsync();
        });
}
