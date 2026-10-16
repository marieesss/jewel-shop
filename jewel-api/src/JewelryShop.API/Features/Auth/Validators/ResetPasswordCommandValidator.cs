using FluentValidation;
using JewelryShop.Features.Auth.Commands;

namespace JewelryShop.Features.Auth.Validators;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(PasswordResetTokenDefaults.InvalidTokenMessage);
        RuleFor(x => x.Password).PasswordPolicy();
    }
}
