using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Controllers;

namespace JewelryShop.API.Tests.Chains;

public class ChainsControllerTests
{
    private readonly FakeSender _sender = new();
    private readonly ChainsController _controller = new();

    public ChainsControllerTests() => _sender.AttachTo(_controller);

    [Fact]
    public async Task Met_a_jour_la_chaine_de_la_route_et_non_celle_du_corps()
    {
        var body = new UpdateChainCommand(99, "Forçat", "", ProductColor.Gold, 5, 20, 45);

        await _controller.Update(7, body, CancellationToken.None);

        var sent = Assert.IsType<UpdateChainCommand>(_sender.LastRequest);
        Assert.Equal(body with { Id = 7 }, sent);
    }
}
