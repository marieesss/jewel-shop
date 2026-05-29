using JewelryShop.Application.Common.Exceptions;
using MediatR;

namespace JewelryShop.Application.Features.Chains.Commands;

public record DeleteChainCommand(long Id) : IRequest<bool>;

public class DeleteChainCommandHandler : IRequestHandler<DeleteChainCommand, bool>
{
    private readonly IChainRepository _chains;

    public DeleteChainCommandHandler(IChainRepository chains) => _chains = chains;

    public async Task<bool> Handle(DeleteChainCommand request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.Id);

        _chains.Delete(chain);
        await _chains.SaveChangesAsync(cancellationToken);

        return true;
    }
}
