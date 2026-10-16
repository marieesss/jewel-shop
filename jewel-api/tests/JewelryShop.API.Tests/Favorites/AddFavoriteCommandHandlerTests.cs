using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.Entities;
using JewelryShop.Features.Favorites.Commands;
using JewelryShop.Features.Favorites.Entities;

namespace JewelryShop.API.Tests.Favorites;

public class AddFavoriteCommandHandlerTests
{
    private const long UserId = 1;
    private const long ChainId = 3;

    private readonly FakeFavoriteRepository _favorites = new();
    private readonly FakeChainRepository _chains = new();
    private readonly AddFavoriteCommandHandler _handler;

    public AddFavoriteCommandHandlerTests()
    {
        _chains.Seed(new Chain { Id = ChainId, Name = "Forçat" });
        _handler = new AddFavoriteCommandHandler(_favorites, _chains, new FakeCharmRepository());
    }

    [Fact]
    public async Task Ajoute_un_produit_en_favori()
    {
        var result = await _handler.Handle(new AddFavoriteCommand(UserId, ChainId, null), CancellationToken.None);

        Assert.Equal(ChainId, result.ChainId);
        Assert.Single(_favorites.All);
        Assert.Equal(1, _favorites.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_un_produit_deja_en_favori()
    {
        _favorites.Seed(new Favorite { Id = 10, UserId = UserId, ChainId = ChainId });

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new AddFavoriteCommand(UserId, ChainId, null), CancellationToken.None));

        Assert.Equal("Ce produit est déjà dans vos favoris.", exception.Message);
        Assert.Equal(0, _favorites.SaveChangesCount);
    }
}
