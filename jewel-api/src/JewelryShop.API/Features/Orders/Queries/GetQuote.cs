using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

public class GetQuoteQueryHandler : IRequestHandler<GetQuoteQuery, QuoteDto>
{
    private readonly NecklaceComposer _composer;

    public GetQuoteQueryHandler(NecklaceComposer composer)
    {
        _composer = composer;
    }

    public async Task<QuoteDto> Handle(GetQuoteQuery request, CancellationToken cancellationToken)
    {
        var necklace = await _composer.ComposeAsync(request, cancellationToken);
        return necklace.ToQuoteDto();
    }
}
