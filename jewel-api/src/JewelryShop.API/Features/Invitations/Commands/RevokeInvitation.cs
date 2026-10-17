using JewelryShop.Common.Exceptions;
using MediatR;

namespace JewelryShop.Features.Invitations.Commands;

/// <summary>Révoque une invitation en attente (dépassée ou non) ou expirée. L'invitation est conservée.</summary>
public class RevokeInvitationCommandHandler : IRequestHandler<RevokeInvitationCommand>
{
    private readonly IInvitationRepository _invitations;
    private readonly TimeProvider _timeProvider;

    public RevokeInvitationCommandHandler(IInvitationRepository invitations, TimeProvider timeProvider)
    {
        _invitations = invitations;
        _timeProvider = timeProvider;
    }

    public async Task Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _invitations.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Invitation introuvable.");

        if (invitation.Status == InvitationStatus.Accepted)
            throw new ConflictException("Cette invitation a déjà été acceptée.");

        if (invitation.Status == InvitationStatus.Revoked)
            throw new ConflictException("Cette invitation est déjà révoquée.");

        invitation.Revoke(_timeProvider.GetUtcNow().UtcDateTime);
        await _invitations.SaveChangesAsync(cancellationToken);
    }
}
