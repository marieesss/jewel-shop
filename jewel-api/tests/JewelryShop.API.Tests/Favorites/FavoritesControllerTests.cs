using JewelryShop.API.Tests.Fakes;
using JewelryShop.Features.Favorites.Commands;
using JewelryShop.Features.Favorites.Controllers;

namespace JewelryShop.API.Tests.Favorites;

public class FavoritesControllerTests
{
    private const long ConnectedUserId = 4;

    private readonly FakeSender _sender = new();
    private readonly FavoritesController _controller = new();

    public FavoritesControllerTests() => _sender.AttachTo(_controller, ConnectedUserId);

    [Fact]
    public async Task Ajoute_le_favori_a_l_utilisateur_connecte_et_non_a_celui_du_corps()
    {
        var body = new AddFavoriteCommand(UserId: 99, ChainId: 3, CharmId: null);

        await _controller.Add(body, CancellationToken.None);

        var sent = Assert.IsType<AddFavoriteCommand>(_sender.LastRequest);
        Assert.Equal(body with { UserId = ConnectedUserId }, sent);
    }
}
