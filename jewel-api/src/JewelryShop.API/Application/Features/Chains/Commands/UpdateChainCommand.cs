using FluentValidation;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Chains.Commands;

public record UpdateChainCommand(
    long Id,
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    decimal Length) : IRequest<ChainDto>;

public class UpdateChainCommandValidator : AbstractValidator<UpdateChainCommand>
{
    public UpdateChainCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.Color).IsInEnum();
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Length).GreaterThan(0);
    }
}

public class UpdateChainCommandHandler : IRequestHandler<UpdateChainCommand, ChainDto>
{
    private readonly IChainRepository _chains;

    public UpdateChainCommandHandler(IChainRepository chains) => _chains = chains;

    public async Task<ChainDto> Handle(UpdateChainCommand request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.Id);

        chain.Name        = request.Name.Trim();
        chain.Description = request.Description?.Trim() ?? string.Empty;
        chain.Color       = request.Color;
        chain.Cost        = request.Cost;
        chain.Price       = request.Price;
        chain.Length      = request.Length;

        _chains.Update(chain);
        await _chains.SaveChangesAsync(cancellationToken);

        return chain.ToDto();
    }
}
