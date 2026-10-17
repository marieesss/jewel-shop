using FluentValidation;
using JewelryShop.Features.Auth.Commands;

namespace JewelryShop.Features.Auth.Validators;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le prénom est requis.")
            .MaximumLength(100).WithMessage("100 caractères maximum.");
        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Le nom est requis.")
            .MaximumLength(100).WithMessage("100 caractères maximum.");
        RuleFor(x => x.InvitationToken)
            .NotEmpty().WithMessage(InvitationDefaults.RequiredInvitationMessage);
        RuleFor(x => x.Password).PasswordPolicy();
        RuleFor(x => x.Birthday)
            .Must(b => b is null || b.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La date de naissance doit être dans le passé.");
    }
}
