using FluentValidation;
using JewelryShop.Features.Orders.Common;

namespace JewelryShop.Features.Orders.Validators;

public static class OrderNoteRules
{
    /// <summary>
    /// Règles du contenu d'une note, communes à l'ajout et à la modification. La longueur est celle du texte
    /// enregistré, sans les espaces de début et de fin.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidNoteContent<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(OrderMessages.NoteRequired)
            .Must(content => content is null || content.Trim().Length <= OrderRules.NoteContentMaxLength)
            .WithMessage(OrderMessages.NoteTooLong);
}
