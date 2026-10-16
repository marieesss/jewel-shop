using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Creations.DTOs;
using MediatR;

namespace JewelryShop.Features.Creations.Queries;

public class GetCreationByIdQueryHandler : IRequestHandler<GetCreationByIdQuery, CreationDto>
{
    private readonly ICreationRepository _creations;

    public GetCreationByIdQueryHandler(ICreationRepository creations) => _creations = creations;

    public async Task<CreationDto> Handle(GetCreationByIdQuery request, CancellationToken cancellationToken)
    {
        var creation = await _creations.GetByIdWithItemsAsync(request.CreationId, cancellationToken)
            ?? throw new NotFoundException("Création", request.CreationId);

        if (creation.UserId != request.UserId)
            throw new ForbiddenAccessException("Cette création ne vous appartient pas.");

        return creation.ToDto();
    }
}
