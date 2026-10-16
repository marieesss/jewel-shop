using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Chains.Queries;

public record GetChainByIdQuery(long Id) : IRequest<ChainDto>;

public class GetChainByIdQueryHandler : IRequestHandler<GetChainByIdQuery, ChainDto>
{
    private readonly IChainRepository _chains;

    public GetChainByIdQueryHandler(IChainRepository chains) => _chains = chains;

    public async Task<ChainDto> Handle(GetChainByIdQuery request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.Id);

        return chain.ToDto();
    }
}
