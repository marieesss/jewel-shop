using JewelryShop.Features.ChainLengths.DTOs;
using MediatR;

namespace JewelryShop.Features.ChainLengths.Queries;

public record GetChainLengthsQuery : IRequest<IReadOnlyList<ChainLengthDto>>;
