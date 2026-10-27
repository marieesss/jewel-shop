using FluentValidation;
using JewelryShop.Features.Tags.Commands;
using JewelryShop.Features.Tags.Common;

namespace JewelryShop.Features.Tags.Validators;

public class RenameTagCommandValidator : AbstractValidator<RenameTagCommand>
{
    public RenameTagCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage(TagMessages.InvalidId);

        RuleFor(x => x.Name).ValidTagName();
    }
}
