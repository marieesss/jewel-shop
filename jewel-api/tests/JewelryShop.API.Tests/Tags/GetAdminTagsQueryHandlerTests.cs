using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Entities;
using JewelryShop.Features.Tags.Queries;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Tags;

public class GetAdminTagsQueryHandlerTests
{
    private readonly FakeTagRepository _tags = new();

    [Fact]
    public async Task Renvoie_pour_chaque_tag_le_nombre_de_chaines_et_de_breloques_qui_le_portent()
    {
        var sea = Tag(1, "Mer");
        foreach (var chainId in new long[] { 10, 11, 12 })
            sea.ChainTags.Add(new ChainTag { ChainId = chainId, TagId = 1 });
        sea.CharmTags.Add(new CharmTag { CharmId = 20, TagId = 1 });
        _tags.Seed(sea, Tag(2, "Doré", TagCategory.Color));
        var handler = new GetAdminTagsQueryHandler(_tags);

        var result = await handler.Handle(new GetAdminTagsQuery(), CancellationToken.None);

        Assert.Equal(
            [new AdminTagDto(1, "Mer", "mer", "Theme", 3, 1), new AdminTagDto(2, "Doré", "dore", "Color", 0, 0)],
            result);
    }
}
