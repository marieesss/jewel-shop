using JewelryShop.Application.Common.Exceptions;
using MediatR;

namespace JewelryShop.Application.Features.Creations.Commands;

public record DeleteCreationCommand(long UserId, long CreationId) : IRequest<bool>;

public class DeleteCreationCommandHandler : IRequestHandler<DeleteCreationCommand, bool>
{
    private readonly ICreationRepository _creations;

    public DeleteCreationCommandHandler(ICreationRepository creations) => _creations = creations;

    public async Task<bool> Handle(DeleteCreationCommand request, CancellationToken cancellationToken)
    {
        var creation = await _creations.GetByIdAsync(request.CreationId, cancellationToken)
            ?? throw new NotFoundException("Création", request.CreationId);

        if (creation.UserId != request.UserId)
            throw new ForbiddenAccessException("Cette création ne t'appartient pas.");

        _creations.Delete(creation);
        await _creations.SaveChangesAsync(cancellationToken);

        return true;
    }
}
