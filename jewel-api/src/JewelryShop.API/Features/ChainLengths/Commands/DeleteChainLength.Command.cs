using MediatR;

namespace JewelryShop.Features.ChainLengths.Commands;

public record DeleteChainLengthCommand(long Id) : IRequest<bool>;
