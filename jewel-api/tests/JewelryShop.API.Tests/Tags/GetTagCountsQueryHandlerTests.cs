using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Entities;
using JewelryShop.Features.Tags.Queries;
using static JewelryShop.API.Tests.Tags.TagFixtures;

namespace JewelryShop.API.Tests.Tags;

public class GetTagCountsQueryHandlerTests
{
    private readonly FakeTagRepository _tags = new();
    private readonly GetTagCountsQueryHandler _handler;

    public GetTagCountsQueryHandlerTests()
    {
        var sea = Tag(1, "Mer");
        foreach (var charmId in Enumerable.Range(100, 10))
            sea.CharmTags.Add(new CharmTag { CharmId = charmId, TagId = 1 });
        sea.ChainTags.Add(new ChainTag { ChainId = 10, TagId = 1 });
        sea.ChainTags.Add(new ChainTag { ChainId = 11, TagId = 1 });
        _tags.Seed(sea, Tag(2, "Doré", TagCategory.Color));
        _handler = new GetTagCountsQueryHandler(_tags);
    }

    [Fact]
    public async Task Compte_les_breloques_qui_portent_chaque_tag()
    {
        var result = await _handler.Handle(new GetTagCountsQuery("charm"), CancellationToken.None);

        Assert.Equal(
            [new TagCountDto(1, "Mer", "mer", "Theme", 10), new TagCountDto(2, "Doré", "dore", "Color", 0)],
            result);
    }

    [Fact]
    public async Task Compte_les_chaines_qui_portent_chaque_tag()
    {
        var result = await _handler.Handle(new GetTagCountsQuery("chain"), CancellationToken.None);

        Assert.Equal(
            [new TagCountDto(1, "Mer", "mer", "Theme", 2), new TagCountDto(2, "Doré", "dore", "Color", 0)],
            result);
    }
}
