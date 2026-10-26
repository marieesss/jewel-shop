using JewelryShop.Common.Exceptions;
using JewelryShop.Features.ChainLengths.Common;
using JewelryShop.Features.ChainLengths.DTOs;
using MediatR;

namespace JewelryShop.Features.ChainLengths.Commands;

public class UpdateChainLengthCommandHandler : IRequestHandler<UpdateChainLengthCommand, ChainLengthDto>
{
    private readonly IChainLengthRepository _lengths;

    public UpdateChainLengthCommandHandler(IChainLengthRepository lengths) => _lengths = lengths;

    public async Task<ChainLengthDto> Handle(UpdateChainLengthCommand request, CancellationToken cancellationToken)
    {
        var length = await _lengths.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ChainLengthMessages.EntityName, request.Id);

        if (await _lengths.ExistsByLengthAsync(request.LengthCm, request.Id, cancellationToken))
            throw new ConflictException(ChainLengthMessages.AlreadyExists);

        length.LengthCm = request.LengthCm;
        length.Label = request.Label.Trim();

        _lengths.Update(length);
        await _lengths.SaveChangesAsync(cancellationToken);

        return length.ToDto();
    }
}
