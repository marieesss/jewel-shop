using System.Reflection;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.ChainLengths.Commands;
using JewelryShop.Features.ChainLengths.Controllers;
using JewelryShop.Features.ChainLengths.DTOs;
using JewelryShop.Features.ChainLengths.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Tests.ChainLengths;

public class AdminChainLengthsControllerTests
{
    private readonly FakeSender _sender = new();
    private readonly AdminChainLengthsController _controller = new();

    public AdminChainLengthsControllerTests() => _sender.AttachTo(_controller);

    [Fact]
    public void Est_reserve_aux_admins_sous_la_route_api_admin_chain_lengths()
    {
        var authorize = typeof(AdminChainLengthsController).GetCustomAttribute<AuthorizeAttribute>();
        var route = typeof(AdminChainLengthsController).GetCustomAttribute<RouteAttribute>();

        Assert.Equal("Admin", authorize?.Roles);
        Assert.Equal("api/admin/chain-lengths", route?.Template);
    }

    [Fact]
    public async Task Renvoie_les_longueurs_et_repond_200()
    {
        IReadOnlyList<ChainLengthDto> lengths = [new ChainLengthDto(1, 38, "Ras du cou")];
        _sender.Response = lengths;

        var response = await _controller.GetAll(CancellationToken.None);

        Assert.IsType<GetChainLengthsQuery>(_sender.LastRequest);
        Assert.Same(lengths, Assert.IsType<OkObjectResult>(response).Value);
    }

    [Fact]
    public async Task Cree_une_longueur_et_repond_201()
    {
        var dto = new ChainLengthDto(10, 48, "Princesse longue");
        _sender.Response = dto;
        var command = new CreateChainLengthCommand(48, "Princesse longue");

        var response = await _controller.Create(command, CancellationToken.None);

        Assert.Same(command, _sender.LastRequest);
        var result = Assert.IsType<ObjectResult>(response);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Same(dto, result.Value);
    }

    [Fact]
    public async Task Modifie_la_longueur_de_la_route_et_repond_200()
    {
        var dto = new ChainLengthDto(7, 46, "Princesse +");
        _sender.Response = dto;

        var response = await _controller.Update(7, new UpdateChainLengthCommand(0, 46, "Princesse +"), CancellationToken.None);

        var sent = Assert.IsType<UpdateChainLengthCommand>(_sender.LastRequest);
        Assert.Equal(7, sent.Id);
        Assert.Same(dto, Assert.IsType<OkObjectResult>(response).Value);
    }

    [Fact]
    public async Task Supprime_la_longueur_de_la_route_et_repond_204()
    {
        _sender.Response = true;

        var response = await _controller.Delete(7, CancellationToken.None);

        Assert.Equal(new DeleteChainLengthCommand(7), _sender.LastRequest);
        Assert.IsType<NoContentResult>(response);
    }
}
