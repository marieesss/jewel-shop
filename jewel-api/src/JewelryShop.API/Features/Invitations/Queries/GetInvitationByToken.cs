using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;
using JewelryShop.Features.Invitations.DTOs;
using MediatR;

namespace JewelryShop.Features.Invitations.Queries;

/// <summary>
/// Renvoie l'email et l'échéance d'une invitation en attente et non expirée. Une invitation inconnue,
/// expirée, révoquée ou acceptée donne le même 404, pour ne rien révéler de son état.
/// </summary>
public class GetInvitationByTokenQueryHandler : IRequestHandler<GetInvitationByTokenQuery, InvitationPreviewDto>
{
    private readonly IInvitationRepository _invitations;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly TimeProvider _timeProvider;

    public GetInvitationByTokenQueryHandler(
        IInvitationRepository invitations,
        ISecureTokenGenerator tokenGenerator,
        TimeProvider timeProvider)
    {
        _invitations = invitations;
        _tokenGenerator = tokenGenerator;
        _timeProvider = timeProvider;
    }

    public async Task<InvitationPreviewDto> Handle(GetInvitationByTokenQuery request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var invitation = await _invitations.GetByTokenHashAsync(_tokenGenerator.Hash(request.Token), cancellationToken);

        if (invitation is null || !invitation.IsAwaitingAcceptance(now))
            throw new NotFoundException(InvitationDefaults.InvalidInvitationMessage);

        return invitation.ToPreviewDto();
    }
}
