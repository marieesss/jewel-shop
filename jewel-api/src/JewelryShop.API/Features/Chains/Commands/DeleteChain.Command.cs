using MediatR;

namespace JewelryShop.Features.Chains.Commands;

public record DeleteChainCommand(long Id) : IRequest<bool>;
