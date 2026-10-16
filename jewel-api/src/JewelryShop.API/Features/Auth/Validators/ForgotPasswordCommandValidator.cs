using FluentValidation;
using JewelryShop.Features.Auth.Commands;

namespace JewelryShop.Features.Auth.Validators;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est requis.")
            .EmailAddress().WithMessage("L'email n'est pas valide.");
    }
}
