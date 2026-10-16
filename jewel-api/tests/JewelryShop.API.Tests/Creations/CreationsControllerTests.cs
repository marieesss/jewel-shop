using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Creations.Commands;
using JewelryShop.Features.Creations.Controllers;
using JewelryShop.Features.Creations.DTOs;

namespace JewelryShop.API.Tests.Creations;

public class CreationsControllerTests
{
    private const long ConnectedUserId = 4;

    private readonly FakeSender _sender = new();
    private readonly CreationsController _controller = new();

    public CreationsControllerTests() => _sender.AttachTo(_controller, ConnectedUserId);

    [Fact]
    public async Task Cree_la_creation_pour_l_utilisateur_connecte_et_non_celui_du_corps()
    {
        _sender.Response = new CreationDto(1, ConnectedUserId, 3, 2, DateTime.UtcNow, null, null, []);
        var body = new CreateCreationCommand(UserId: 99, ChainId: 3, OptionId: 2, Items: [new CreateCreationItemInput(5, 0)]);

        await _controller.Create(body, CancellationToken.None);

        var sent = Assert.IsType<CreateCreationCommand>(_sender.LastRequest);
        Assert.Equal(ConnectedUserId, sent.UserId);
        Assert.Equal(body.Items, sent.Items);
    }
}
