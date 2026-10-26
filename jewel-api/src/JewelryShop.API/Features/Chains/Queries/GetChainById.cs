using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Queries;

public class GetChainByIdQueryHandler : IRequestHandler<GetChainByIdQuery, ChainDto>
{
    private readonly IChainRepository _chains;
    private readonly IChainLengthRepository _lengths;

    public GetChainByIdQueryHandler(IChainRepository chains, IChainLengthRepository lengths)
    {
        _chains = chains;
        _lengths = lengths;
    }

    public async Task<ChainDto> Handle(GetChainByIdQuery request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdWithRollsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.Id);

        return chain.ToDto(await _lengths.GetAllOrderedAsync(cancellationToken));
    }
}
