using FluentValidation;
using JewelryShop.Features.Chains.Commands;
using JewelryShop.Features.Chains.Common;
using JewelryShop.Features.Tags.Common;

namespace JewelryShop.Features.Chains.Validators;

public class CreateChainCommandValidator : AbstractValidator<CreateChainCommand>
{
    public const string InitialRollLengthMessage = "La longueur du rouleau doit être supérieure à 0.";

    public CreateChainCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom est requis.")
            .MaximumLength(255).WithMessage("255 caractères maximum.");
        RuleFor(x => x.Description)
            .MaximumLength(10000).WithMessage("10000 caractères maximum.");
        RuleFor(x => x.Color)
            .IsInEnum().WithMessage("Couleur invalide.");
        Include(new ChainPricesValidator<CreateChainCommand>());
        RuleFor(x => x.InitialRollLengthCm)
            .NotNull().WithMessage(InitialRollLengthMessage)
            .GreaterThan(0).WithMessage(InitialRollLengthMessage)
            .When(x => x.UsesMeterPrices());
        RuleFor(x => x.Url)
            .MaximumLength(500).WithMessage("500 caractères maximum.");
        this.RuleForTagIds(x => x.TagIds);
    }
}
