using System.Reflection;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Tags.Commands;
using JewelryShop.Features.Tags.Controllers;
using JewelryShop.Features.Tags.DTOs;
using JewelryShop.Features.Tags.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

    [Fact]
    public async Task Cree_un_tag_et_repond_201()
    {
        var dto = new TagDto(10, "Mer", "mer", "Theme");
        _sender.Response = dto;
        var command = new CreateTagCommand("Mer", "Theme");

        var response = await _controller.Create(command, CancellationToken.None);

        Assert.Same(command, _sender.LastRequest);
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Same(dto, result.Value);
    }

    [Fact]
    public async Task Renomme_le_tag_de_la_route_et_repond_200()
    {
        var dto = new TagDto(7, "Océan", "ocean", "Theme");
        _sender.Response = dto;

        var response = await _controller.Rename(7, new RenameTagCommand(0, "Océan"), CancellationToken.None);

        var sent = Assert.IsType<RenameTagCommand>(_sender.LastRequest);
        Assert.Equal(7, sent.Id);
        Assert.Equal("Océan", sent.Name);
        Assert.Same(dto, Assert.IsType<OkObjectResult>(response).Value);
    }

    [Fact]
    public async Task Supprime_le_tag_de_la_route_et_repond_204()
    {
        _sender.Response = true;

        var response = await _controller.Delete(7, CancellationToken.None);

        Assert.Equal(new DeleteTagCommand(7), _sender.LastRequest);
        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public async Task Fusionne_le_tag_de_la_route_et_repond_200_avec_le_tag_cible()
    {
        var dto = new TagDto(3, "Mer", "mer", "Theme");
        _sender.Response = dto;

        var response = await _controller.Merge(7, new MergeTagCommand(0, 3), CancellationToken.None);

        Assert.Equal(new MergeTagCommand(7, 3), _sender.LastRequest);
        Assert.Same(dto, Assert.IsType<OkObjectResult>(response).Value);
    }
}
