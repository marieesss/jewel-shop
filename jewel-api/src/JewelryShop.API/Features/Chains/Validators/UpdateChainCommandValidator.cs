using FluentValidation;
using JewelryShop.Features.Chains.Commands;

namespace JewelryShop.Features.Chains.Validators;

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
        Include(new ChainPricesValidator<UpdateChainCommand>());
        RuleFor(x => x.Url)
            .MaximumLength(500).WithMessage("500 caractères maximum.");
    }
}
