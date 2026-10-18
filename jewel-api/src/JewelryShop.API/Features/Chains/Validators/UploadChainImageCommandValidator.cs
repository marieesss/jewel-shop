using FluentValidation;
using JewelryShop.Common.Validation;
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
            .OverridePropertyName(ImageFileRules.PropertyName);

        When(x => x.Length > 0, () =>
        {
            RuleFor(x => x.ContentType)
                .Must(ImageFileRules.IsAllowedContentType).WithMessage(ImageFileRules.ContentTypeMessage)
                .OverridePropertyName(ImageFileRules.PropertyName);
            RuleFor(x => x.FileName)
                .Must((command, fileName) => ImageFileRules.ExtensionMatchesContentType(fileName, command.ContentType))
                .WithMessage(ImageFileRules.ExtensionMessage)
                .When(x => ImageFileRules.IsAllowedContentType(x.ContentType))
                .OverridePropertyName(ImageFileRules.PropertyName);
            RuleFor(x => x.Length)
                .LessThanOrEqualTo(ImageFileRules.MaxSizeInBytes).WithMessage(ImageFileRules.MaxSizeMessage)
                .OverridePropertyName(ImageFileRules.PropertyName);
        });
    }
}
