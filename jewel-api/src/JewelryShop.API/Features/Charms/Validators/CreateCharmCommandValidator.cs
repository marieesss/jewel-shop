using FluentValidation;
using JewelryShop.Features.Charms.Commands;
using JewelryShop.Features.Tags.Common;

namespace JewelryShop.Features.Charms.Validators;

public class CreateCharmCommandValidator : AbstractValidator<CreateCharmCommand>
{
    public CreateCharmCommandValidator()
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
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Le stock doit être positif.");
        RuleFor(x => x.Url)
            .MaximumLength(500).WithMessage("500 caractères maximum.");
        this.RuleForTagIds(x => x.TagIds);
    }
}
