using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Commands;

public record CreateChainCommand(
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    decimal Length,
    string? Url = null) : IRequest<ChainDto>;

public class CreateChainCommandHandler : IRequestHandler<CreateChainCommand, ChainDto>
{
    private readonly IChainRepository _chains;

    public CreateChainCommandHandler(IChainRepository chains) => _chains = chains;

    public async Task<ChainDto> Handle(CreateChainCommand request, CancellationToken cancellationToken)
    {
        var chain = new Chain
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Color = request.Color,
            Cost = request.Cost,
            Price = request.Price,
            Length = request.Length,
            Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim()
        };

        await _chains.AddAsync(chain, cancellationToken);
        await _chains.SaveChangesAsync(cancellationToken);

        return chain.ToDto();
    }
}
