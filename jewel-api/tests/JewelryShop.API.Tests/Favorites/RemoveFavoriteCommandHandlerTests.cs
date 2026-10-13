using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Favorites;
using JewelryShop.Application.Features.Favorites.Commands;

namespace JewelryShop.API.Tests.Favorites;

public class RemoveFavoriteCommandHandlerTests
{
    private const long OwnerId = 1;
    private const long OtherUserId = 2;
    private const long FavoriteId = 10;

    private readonly FakeFavoriteRepository _favorites = new();
    private readonly RemoveFavoriteCommandHandler _handler;

    public RemoveFavoriteCommandHandlerTests()
    {
        _favorites.Seed(new Favorite { Id = FavoriteId, UserId = OwnerId, ChainId = 3 });
        _handler = new RemoveFavoriteCommandHandler(_favorites);
    }

    [Fact]
    public async Task Supprime_un_favori_de_l_utilisateur()
    {
        await _handler.Handle(new RemoveFavoriteCommand(OwnerId, FavoriteId), CancellationToken.None);

        Assert.Empty(_favorites.All);
        Assert.Equal(1, _favorites.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_supprimer_le_favori_d_un_autre_utilisateur()
    {
        var exception = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _handler.Handle(new RemoveFavoriteCommand(OtherUserId, FavoriteId), CancellationToken.None));

        Assert.Equal("Ce favori ne vous appartient pas.", exception.Message);
        Assert.Single(_favorites.All);
    }
}
