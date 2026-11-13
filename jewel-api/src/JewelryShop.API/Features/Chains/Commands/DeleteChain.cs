using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Common;
using MediatR;

namespace JewelryShop.Features.Chains.Commands;

public class DeleteChainCommandHandler : IRequestHandler<DeleteChainCommand, bool>
{
    private readonly IChainRepository _chains;
    private readonly IOrderRepository _orders;

    public DeleteChainCommandHandler(IChainRepository chains, IOrderRepository orders)
    {
        _chains = chains;
        _orders = orders;
    }

    public async Task<bool> Handle(DeleteChainCommand request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.Id);

        if (await _orders.ContainsChainAsync(chain.Id, cancellationToken))
            throw new ConflictException(OrderMessages.ProductInOrder);

        _chains.Delete(chain);
        await _chains.SaveChangesAsync(cancellationToken);

        return true;
    }
}
