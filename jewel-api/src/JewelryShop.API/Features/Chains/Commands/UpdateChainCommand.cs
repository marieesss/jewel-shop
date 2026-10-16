using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Commands;

public record UpdateChainCommand(
    long Id,
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    decimal Length,
    string? Url = null) : IRequest<ChainDto>;

public class UpdateChainCommandHandler : IRequestHandler<UpdateChainCommand, ChainDto>
{
    private readonly IChainRepository _chains;

    public UpdateChainCommandHandler(IChainRepository chains) => _chains = chains;

    public async Task<ChainDto> Handle(UpdateChainCommand request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.Id);

        chain.Name = request.Name.Trim();
        chain.Description = request.Description?.Trim() ?? string.Empty;
        chain.Color = request.Color;
        chain.Cost = request.Cost;
        chain.Price = request.Price;
        chain.Length = request.Length;
        chain.Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim();

        _chains.Update(chain);
        await _chains.SaveChangesAsync(cancellationToken);

        return chain.ToDto();
    }
}
