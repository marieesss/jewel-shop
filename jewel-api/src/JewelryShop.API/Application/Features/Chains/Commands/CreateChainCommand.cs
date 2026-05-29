using FluentValidation;
using JewelryShop.Application.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Chains.Commands;

public record CreateChainCommand(
    string Name,
    string Description,
    ProductColor Color,
    string? Url,
    decimal Cost,
    decimal Price,
    decimal Length) : IRequest<ChainDto>;

public class CreateChainCommandValidator : AbstractValidator<CreateChainCommand>
{
    public CreateChainCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.Color).IsInEnum();
        RuleFor(x => x.Url).MaximumLength(500);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Length).GreaterThan(0);
    }
}

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
            Url = request.Url?.Trim(),
            Cost = request.Cost,
            Price = request.Price,
            Length = request.Length
        };

        await _chains.AddAsync(chain, cancellationToken);
        await _chains.SaveChangesAsync(cancellationToken);

        return chain.ToDto();
    }
}
