using System.Reflection;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Tags.Controllers;
using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Tests.Tags;

public class AdminTagsControllerTests
{
    private readonly FakeSender _sender = new();
    private readonly AdminTagsController _controller = new();

    public AdminTagsControllerTests() => _sender.AttachTo(_controller);

    [Fact]
    public void Est_reserve_aux_admins_sous_la_route_api_admin_tags()
    {
        var authorize = typeof(AdminTagsController).GetCustomAttribute<AuthorizeAttribute>();
        var route = typeof(AdminTagsController).GetCustomAttribute<RouteAttribute>();

        Assert.Equal("Admin", authorize?.Roles);
        Assert.Equal("api/admin/tags", route?.Template);
    }

    [Fact]
    public async Task Renvoie_les_tags_avec_leur_usage_et_repond_200()
    {
        IReadOnlyList<AdminTagDto> tags = [new AdminTagDto(1, "Mer", "mer", "Theme", 3, 0)];
        _sender.Response = tags;

        var response = await _controller.GetAll(CancellationToken.None);

        Assert.IsType<GetAdminTagsQuery>(_sender.LastRequest);
        Assert.Same(tags, Assert.IsType<OkObjectResult>(response).Value);
    }
}
