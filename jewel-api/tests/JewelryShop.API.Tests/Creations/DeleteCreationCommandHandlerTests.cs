using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Creations;
using JewelryShop.Application.Features.Creations.Commands;

namespace JewelryShop.API.Tests.Creations;

public class DeleteCreationCommandHandlerTests
{
    private const long OwnerId = 1;
    private const long OtherUserId = 2;
    private const long CreationId = 10;
    private const long UnknownCreationId = 99;

    private readonly FakeCreationRepository _creations = new();
    private readonly DeleteCreationCommandHandler _handler;

    public DeleteCreationCommandHandlerTests()
    {
        _creations.Seed(new Creation { Id = CreationId, UserId = OwnerId, ChainId = 3, OptionId = 4 });
        _handler = new DeleteCreationCommandHandler(_creations);
    }

    [Fact]
    public async Task Supprime_une_creation_de_l_utilisateur()
    {
        await _handler.Handle(new DeleteCreationCommand(OwnerId, CreationId), CancellationToken.None);

        Assert.Empty(_creations.All);
        Assert.Equal(1, _creations.SaveChangesCount);
    }

    [Fact]
    public async Task Refuse_de_supprimer_la_creation_d_un_autre_utilisateur()
    {
        var exception = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _handler.Handle(new DeleteCreationCommand(OtherUserId, CreationId), CancellationToken.None));

        Assert.Equal("Cette création ne vous appartient pas.", exception.Message);
        Assert.Single(_creations.All);
        Assert.Equal(0, _creations.SaveChangesCount);
    }

    [Fact]
    public async Task Signale_une_creation_inexistante()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new DeleteCreationCommand(OwnerId, UnknownCreationId), CancellationToken.None));
    }
}
