using FluentValidation;
using JewelryShop.Features.Tags.Commands;
using JewelryShop.Features.Tags.Common;

namespace JewelryShop.Features.Tags.Validators;

public class MergeTagCommandValidator : AbstractValidator<MergeTagCommand>
{
    public MergeTagCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(TagMessages.InvalidId);

        RuleFor(x => x.TargetTagId)
            .GreaterThan(0).WithMessage(TagMessages.InvalidId)
            .NotEqual(x => x.Id).WithMessage(TagMessages.CannotMergeWithItself);
    }
}
