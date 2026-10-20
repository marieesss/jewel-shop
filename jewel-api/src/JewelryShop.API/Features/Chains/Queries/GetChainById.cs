using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Queries;

public class GetChainByIdQueryHandler : IRequestHandler<GetChainByIdQuery, ChainDto>
{
    private readonly IChainRepository _chains;

    public GetChainByIdQueryHandler(IChainRepository chains) => _chains = chains;

    public async Task<ChainDto> Handle(GetChainByIdQuery request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdWithRollsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.Id);

        return chain.ToDto(request.IncludeAdminFields);
    }
}
