using JewelryShop.API.Tests.Fakes;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Creations;
using JewelryShop.Application.Features.Creations.Queries;

namespace JewelryShop.API.Tests.Creations;

public class GetCreationByIdQueryHandlerTests
{
    private const long OwnerId = 1;
    private const long OtherUserId = 2;
    private const long CreationId = 10;
    private const long UnknownCreationId = 99;

    private readonly FakeCreationRepository _creations = new();
    private readonly GetCreationByIdQueryHandler _handler;

    public GetCreationByIdQueryHandlerTests()
    {
        _creations.Seed(new Creation { Id = CreationId, UserId = OwnerId, ChainId = 3, OptionId = 4 });
        _handler = new GetCreationByIdQueryHandler(_creations);
    }

    [Fact]
    public async Task Renvoie_la_creation_de_l_utilisateur()
    {
        var creation = await _handler.Handle(new GetCreationByIdQuery(OwnerId, CreationId), CancellationToken.None);

        Assert.Equal(CreationId, creation.Id);
        Assert.Equal(OwnerId, creation.UserId);
    }

    [Fact]
    public async Task Refuse_de_consulter_la_creation_d_un_autre_utilisateur()
    {
        var exception = await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _handler.Handle(new GetCreationByIdQuery(OtherUserId, CreationId), CancellationToken.None));

        Assert.Equal("Cette création ne vous appartient pas.", exception.Message);
    }

    [Fact]
    public async Task Signale_une_creation_inexistante()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetCreationByIdQuery(OwnerId, UnknownCreationId), CancellationToken.None));
    }
}
