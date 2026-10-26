using System.Reflection;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Tags.Controllers;
using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Tests.Tags;

public class TagsControllerTests
{
    private readonly FakeSender _sender = new();
    private readonly TagsController _controller = new();

    public TagsControllerTests() => _sender.AttachTo(_controller);

    [Fact]
    public void Est_public_sous_la_route_api_tags()
    {
        var authorize = typeof(TagsController).GetCustomAttribute<AuthorizeAttribute>();
        var route = typeof(TagsController).GetCustomAttribute<RouteAttribute>();

        Assert.Null(authorize);
        Assert.Equal("api/tags", route?.Template);
    }

    [Fact]
    public async Task Renvoie_les_tags_et_repond_200()
    {
        IReadOnlyList<TagDto> tags = [new TagDto(1, "Mer", "mer", "Theme")];
        _sender.Response = tags;

        var response = await _controller.GetAll(CancellationToken.None);

        Assert.IsType<GetTagsQuery>(_sender.LastRequest);
        Assert.Same(tags, Assert.IsType<OkObjectResult>(response).Value);
    }
}
