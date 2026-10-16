using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Features.Charms.Queries;

public record GetCharmByIdQuery(long Id) : IRequest<CharmDto>;

public class GetCharmByIdQueryHandler : IRequestHandler<GetCharmByIdQuery, CharmDto>
{
    private readonly ICharmRepository _charms;

    public GetCharmByIdQueryHandler(ICharmRepository charms) => _charms = charms;

    public async Task<CharmDto> Handle(GetCharmByIdQuery request, CancellationToken cancellationToken)
    {
        var charm = await _charms.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Breloque", request.Id);

        return charm.ToDto();
    }
}
