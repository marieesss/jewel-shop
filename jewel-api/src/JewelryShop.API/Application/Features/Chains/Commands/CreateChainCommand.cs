using FluentValidation;
using JewelryShop.Application.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Chains.Commands;

public record CreateChainCommand(
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    decimal Length,
    string? Url = null) : IRequest<ChainDto>;

public class CreateChainCommandValidator : AbstractValidator<CreateChainCommand>
{
    public CreateChainCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom est requis.")
            .MaximumLength(255).WithMessage("255 caractères maximum.");
        RuleFor(x => x.Description)
            .MaximumLength(10000).WithMessage("10000 caractères maximum.");
        RuleFor(x => x.Color)
            .IsInEnum().WithMessage("Couleur invalide.");
        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Le coût doit être positif.");
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix doit être positif.");
        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("La longueur doit être supérieure à 0.");
        RuleFor(x => x.Url)
            .MaximumLength(500).WithMessage("500 caractères maximum.");
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
