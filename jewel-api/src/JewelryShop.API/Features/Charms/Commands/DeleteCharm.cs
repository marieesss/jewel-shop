using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Orders.Common;
using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public class DeleteCharmCommandHandler : IRequestHandler<DeleteCharmCommand, bool>
{
    private readonly ICharmRepository _charms;
    private readonly IOrderRepository _orders;

    public DeleteCharmCommandHandler(ICharmRepository charms, IOrderRepository orders)
    {
        _charms = charms;
        _orders = orders;
    }

    public async Task<bool> Handle(DeleteCharmCommand request, CancellationToken cancellationToken)
    {
        var charm = await _charms.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Breloque", request.Id);

        if (await _orders.ContainsCharmAsync(charm.Id, cancellationToken))
            throw new ConflictException(OrderMessages.ProductInOrder);

        _charms.Delete(charm);
        await _charms.SaveChangesAsync(cancellationToken);

        return true;
    }
}
