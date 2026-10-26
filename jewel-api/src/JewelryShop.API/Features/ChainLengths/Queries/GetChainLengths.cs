using JewelryShop.Features.ChainLengths.DTOs;
using MediatR;

namespace JewelryShop.Features.ChainLengths.Queries;

public class GetChainLengthsQueryHandler : IRequestHandler<GetChainLengthsQuery, IReadOnlyList<ChainLengthDto>>
{
    private readonly IChainLengthRepository _lengths;

    public GetChainLengthsQueryHandler(IChainLengthRepository lengths) => _lengths = lengths;

    public async Task<IReadOnlyList<ChainLengthDto>> Handle(GetChainLengthsQuery request, CancellationToken cancellationToken)
    {
        var lengths = await _lengths.GetAllOrderedAsync(cancellationToken);
        return lengths.Select(l => l.ToDto()).ToList();
    }
}
