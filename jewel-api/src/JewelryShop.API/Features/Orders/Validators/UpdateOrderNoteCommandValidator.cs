using FluentValidation;
using JewelryShop.Features.Orders.Commands;

namespace JewelryShop.Features.Orders.Validators;

public class UpdateOrderNoteCommandValidator : AbstractValidator<UpdateOrderNoteCommand>
{
    public UpdateOrderNoteCommandValidator()
    {
        RuleFor(x => x.Content).ValidNoteContent();
    }
}
