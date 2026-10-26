using FluentValidation;
using JewelryShop.Features.ChainLengths.Common;

namespace JewelryShop.Features.ChainLengths.Validators;

/// <summary>Règles d'une longueur de chaîne, communes à la création et à la modification.</summary>
public class ChainLengthFieldsValidator<T> : AbstractValidator<T> where T : IChainLengthFields
{
    public ChainLengthFieldsValidator()
    {
        RuleFor(x => x.LengthCm)
            .InclusiveBetween(ChainLengthRules.MinLengthCm, ChainLengthRules.MaxLengthCm)
            .WithMessage(ChainLengthMessages.LengthOutOfRange);

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage(ChainLengthMessages.LabelRequired)
            .MaximumLength(ChainLengthRules.LabelMaxLength).WithMessage(ChainLengthMessages.LabelTooLong);
    }
}
