using JewelryShop.Common.Exceptions;
using JewelryShop.Features.ChainLengths.Common;
using JewelryShop.Features.ChainLengths.DTOs;
using MediatR;

namespace JewelryShop.Features.ChainLengths.Commands;

public class CreateChainLengthCommandHandler : IRequestHandler<CreateChainLengthCommand, ChainLengthDto>
{
    private readonly IChainLengthRepository _lengths;

    public CreateChainLengthCommandHandler(IChainLengthRepository lengths) => _lengths = lengths;

    public async Task<ChainLengthDto> Handle(CreateChainLengthCommand request, CancellationToken cancellationToken)
    {
        if (await _lengths.ExistsByLengthAsync(request.LengthCm, cancellationToken: cancellationToken))
            throw new ConflictException(ChainLengthMessages.AlreadyExists);

        var length = new ChainLength { LengthCm = request.LengthCm, Label = request.Label.Trim() };

        await _lengths.AddAsync(length, cancellationToken);
        await _lengths.SaveChangesAsync(cancellationToken);

        return length.ToDto();
    }
}
