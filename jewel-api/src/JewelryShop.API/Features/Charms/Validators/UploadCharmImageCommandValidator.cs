using FluentValidation;
using JewelryShop.Features.Charms.Commands;

namespace JewelryShop.Features.Charms.Validators;

public class UploadCharmImageCommandValidator : AbstractValidator<UploadCharmImageCommand>
{
    public UploadCharmImageCommandValidator()
    {
        RuleFor(x => x.CharmId)
            .GreaterThan(0).WithMessage("Identifiant invalide.");
        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Aucun fichier fourni.")
            .OverridePropertyName("File");
    }
}
