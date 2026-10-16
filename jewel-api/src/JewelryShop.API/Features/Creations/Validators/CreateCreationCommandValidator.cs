using FluentValidation;
using JewelryShop.Features.Creations.Commands;

namespace JewelryShop.Features.Creations.Validators;

public class CreateCreationCommandValidator : AbstractValidator<CreateCreationCommand>
{
    public CreateCreationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Identifiant invalide.");
        RuleFor(x => x.ChainId)
            .GreaterThan(0).WithMessage("Identifiant invalide.");
        RuleFor(x => x.OptionId)
            .GreaterThan(0).WithMessage("Identifiant invalide.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Une création doit contenir au moins une breloque.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.CharmId)
                .GreaterThan(0).WithMessage("Identifiant invalide.");
            item.RuleFor(i => i.Position)
                .GreaterThanOrEqualTo(0).WithMessage("La position doit être positive.");
        });

        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.Position).Distinct().Count() == items.Count)
            .WithMessage("Les positions des breloques doivent être uniques.")
            .When(x => x.Items is { Count: > 0 });
    }
}
