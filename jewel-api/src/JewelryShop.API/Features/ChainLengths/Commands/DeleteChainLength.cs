using JewelryShop.Common.Exceptions;
using JewelryShop.Features.ChainLengths.Common;
using MediatR;

namespace JewelryShop.Features.ChainLengths.Commands;

public class DeleteChainLengthCommandHandler : IRequestHandler<DeleteChainLengthCommand, bool>
{
    private readonly IChainLengthRepository _lengths;

    public DeleteChainLengthCommandHandler(IChainLengthRepository lengths) => _lengths = lengths;

    public async Task<bool> Handle(DeleteChainLengthCommand request, CancellationToken cancellationToken)
    {
        var length = await _lengths.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ChainLengthMessages.EntityName, request.Id);

        _lengths.Delete(length);
        await _lengths.SaveChangesAsync(cancellationToken);

        return true;
    }
}
