using FluentValidation;
using JewelryShop.Features.Tags.Commands;
using JewelryShop.Features.Tags.Common;

namespace JewelryShop.Features.Tags.Validators;

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name).ValidTagName();

        RuleFor(x => x.Category)
            .Must(TagCategories.IsValid).WithMessage(TagMessages.InvalidCategory);
    }
}
