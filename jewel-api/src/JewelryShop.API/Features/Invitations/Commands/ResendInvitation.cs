using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;
using JewelryShop.Features.Invitations.DTOs;
using MediatR;
using Microsoft.Extensions.Options;

namespace JewelryShop.Features.Invitations.Commands;

/// <summary>
/// Renvoie une invitation en attente, expirée ou révoquée : nouveau token, 7 jours de validité et nouveau mail,
/// enregistrés dans la même sauvegarde. L'admin qui a invité et la date de création ne changent pas.
/// </summary>
public class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand, InvitationDto>
{
    private readonly IInvitationRepository _invitations;
    private readonly IUserRepository _users;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly IEmailQueue _emailQueue;
    private readonly AppOptions _appOptions;
    private readonly TimeProvider _timeProvider;

    public ResendInvitationCommandHandler(
        IInvitationRepository invitations,
        IUserRepository users,
        ISecureTokenGenerator tokenGenerator,
        IEmailQueue emailQueue,
        IOptions<AppOptions> appOptions,
        TimeProvider timeProvider)
    {
        _invitations = invitations;
        _users = users;
        _tokenGenerator = tokenGenerator;
        _emailQueue = emailQueue;
        _appOptions = appOptions.Value;
        _timeProvider = timeProvider;
    }

    public async Task<InvitationDto> Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var invitation = await _invitations.GetByIdWithInviterAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Invitation introuvable.");

        if (invitation.Status == InvitationStatus.Accepted)
            throw new ConflictException("Cette invitation a déjà été acceptée.");

        if (await _users.ExistsByEmailAsync(invitation.Email, cancellationToken))
            throw new ConflictException("Un compte existe déjà avec cet email.");

        await ExpireOtherOutdatedPendingInvitationsAsync(invitation, now, cancellationToken);

        var token = _tokenGenerator.Generate();
        invitation.Renew(_tokenGenerator.Hash(token), now);
        InvitationEmail.Enqueue(_emailQueue, _appOptions, invitation, token);

        await _invitations.SaveChangesAsync(cancellationToken);

        return invitation.ToDto(now);
    }

    // Comme à la création : une autre invitation encore valable pour cet email bloque le renvoi,
    // une autre invitation dépassée passe à Expired dans la même sauvegarde.
    private async Task ExpireOtherOutdatedPendingInvitationsAsync(Invitation invitation, DateTime now, CancellationToken cancellationToken)
    {
        var others = (await _invitations.GetPendingByEmailAsync(invitation.Email, cancellationToken))
            .Where(i => i.Id != invitation.Id)
            .ToList();

        if (others.Any(i => i.IsAwaitingAcceptance(now)))
            throw new ConflictException("Une invitation est déjà en attente pour cet email.");

        foreach (var outdated in others)
            outdated.MarkExpired();
    }
}
