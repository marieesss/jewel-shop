using FluentValidation;
using JewelryShop.Features.Chains.Commands;

namespace JewelryShop.Features.Chains.Validators;

public class UploadChainImageCommandValidator : AbstractValidator<UploadChainImageCommand>
{
    public UploadChainImageCommandValidator()
    {
        RuleFor(x => x.ChainId)
            .GreaterThan(0).WithMessage("Identifiant invalide.");
        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Aucun fichier fourni.")
            .OverridePropertyName("File");
    }
}
