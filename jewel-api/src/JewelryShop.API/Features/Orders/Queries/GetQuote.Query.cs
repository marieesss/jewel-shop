using JewelryShop.Features.Orders.Common;
using JewelryShop.Features.Orders.DTOs;
using MediatR;

namespace JewelryShop.Features.Orders.Queries;

public record GetQuoteQuery(long ChainId, int LengthCm, IReadOnlyList<long> CharmIds) : IRequest<QuoteDto>, INecklaceComposition;
