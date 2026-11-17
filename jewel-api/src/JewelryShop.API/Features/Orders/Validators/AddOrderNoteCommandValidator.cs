using FluentValidation;
using JewelryShop.Features.Orders.Commands;

namespace JewelryShop.Features.Orders.Validators;

public class AddOrderNoteCommandValidator : AbstractValidator<AddOrderNoteCommand>
{
    public AddOrderNoteCommandValidator()
    {
        RuleFor(x => x.Content).ValidNoteContent();
    }
}
