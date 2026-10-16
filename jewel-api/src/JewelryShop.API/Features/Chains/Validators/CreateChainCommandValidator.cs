using FluentValidation;
using JewelryShop.Features.Chains.Commands;

namespace JewelryShop.Features.Chains.Validators;

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
