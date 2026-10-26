using System.Reflection;
using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.ChainLengths.Controllers;
using JewelryShop.Features.ChainLengths.DTOs;
using JewelryShop.Features.ChainLengths.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryShop.API.Tests.ChainLengths;

public class ChainLengthsControllerTests
{
    private readonly FakeSender _sender = new();
    private readonly ChainLengthsController _controller = new();

    public ChainLengthsControllerTests() => _sender.AttachTo(_controller);

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
    public void Est_public_sous_la_route_api_chain_lengths()
    {
        var authorize = typeof(ChainLengthsController).GetCustomAttribute<AuthorizeAttribute>();
        var route = typeof(ChainLengthsController).GetCustomAttribute<RouteAttribute>();

        Assert.Null(authorize);
        Assert.Equal("api/chain-lengths", route?.Template);
    }
}
