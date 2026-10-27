using FluentValidation;
using JewelryShop.Features.Tags.Common;

namespace JewelryShop.Features.Tags.Validators;

public static class TagNameRules
{
    /// <summary>Règles du nom d'un tag, communes à la création et au renommage.</summary>
    public static IRuleBuilderOptions<T, string> ValidTagName<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(TagMessages.NameRequired)
            .Must(name => name is null || name.Trim().Length <= TagRules.NameMaxLength).WithMessage(TagMessages.NameTooLong)
            .Must(name => string.IsNullOrWhiteSpace(name) || TagSlug.CanBeBuiltFrom(name)).WithMessage(TagMessages.NameWithoutLetterOrDigit);
}
