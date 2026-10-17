using FluentValidation;
using JewelryShop.Features.Invitations.Queries;

namespace JewelryShop.Features.Invitations.Validators;

public class GetInvitationsQueryValidator : AbstractValidator<GetInvitationsQuery>
{
    public GetInvitationsQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => InvitationQueries.TryParseStatus(status, out _))
            .WithMessage("Le statut n'est pas valide.");
    }
}
