using FluentValidation;
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

public class UpdateChainCommandValidator : AbstractValidator<UpdateChainCommand>
{
    public UpdateChainCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Identifiant invalide.");
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
