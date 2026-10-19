using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Commands;

public class UpdateChainCommandHandler : IRequestHandler<UpdateChainCommand, ChainDto>
{
    private readonly IChainRepository _chains;

    public UpdateChainCommandHandler(IChainRepository chains) => _chains = chains;

    public async Task<ChainDto> Handle(UpdateChainCommand request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdWithRollsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.Id);

        chain.Name = request.Name.Trim();
        chain.Description = request.Description?.Trim() ?? string.Empty;
        chain.Color = request.Color;
        chain.ApplyPrices(request);
        chain.Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim();

        _chains.Update(chain);
        await _chains.SaveChangesAsync(cancellationToken);

        return chain.ToDto();
    }
}
