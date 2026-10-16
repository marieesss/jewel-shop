using FluentValidation;
using JewelryShop.Features.Invitations.Commands;

namespace JewelryShop.Features.Invitations.Validators;

public class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est requis.")
            .EmailAddress().WithMessage("L'email n'est pas valide.")
            .MaximumLength(255).WithMessage("255 caractères maximum.");
    }
}
