using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Charms.Commands;
using JewelryShop.Features.Charms.Controllers;

namespace JewelryShop.API.Tests.Charms;

public class CharmsControllerTests
{
    private readonly FakeSender _sender = new();
    private readonly CharmsController _controller = new();

    public CharmsControllerTests() => _sender.AttachTo(_controller);

    [Fact]
    public async Task Met_a_jour_la_breloque_de_la_route_et_non_celle_du_corps()
    {
        var body = new UpdateCharmCommand(99, "Trèfle", "", ProductColor.Silver, 2, 8, 10);

        await _controller.Update(7, body, CancellationToken.None);

        var sent = Assert.IsType<UpdateCharmCommand>(_sender.LastRequest);
        Assert.Equal(body with { Id = 7 }, sent);
    }
}
