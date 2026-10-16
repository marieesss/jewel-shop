using System.Globalization;
using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;
using JewelryShop.Features.Invitations.DTOs;
using JewelryShop.Infrastructure.Email;
using MediatR;
using Microsoft.Extensions.Options;

namespace JewelryShop.Features.Invitations.Commands;

/// <summary>
/// Crée une invitation valable 7 jours et met en file le mail qui contient le lien d'inscription.
/// L'invitation et le mail sont enregistrés dans la même sauvegarde.
/// </summary>
public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, InvitationDto>
{
    private const string EmailDateFormat = "dd/MM/yyyy";

    private readonly IInvitationRepository _invitations;
    private readonly IUserRepository _users;
    private readonly ISecureTokenGenerator _tokenGenerator;
    private readonly IEmailQueue _emailQueue;
    private readonly AppOptions _appOptions;
    private readonly TimeProvider _timeProvider;

    public CreateInvitationCommandHandler(
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

    public async Task<InvitationDto> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
            throw new ConflictException("Un compte existe déjà avec cet email.");

        await ExpireOutdatedPendingInvitationsAsync(email, now, cancellationToken);

        var admin = await _users.GetByIdAsync(request.InvitedById, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.InvitedById);

        var token = _tokenGenerator.Generate();
        var invitation = Invitation.Create(email, _tokenGenerator.Hash(token), admin, now);
        _invitations.Add(invitation);
        _emailQueue.Enqueue(email, EmailTemplates.Invitation, new
        {
            invitation.InviterName,
            RegisterUrl = _appOptions.FrontendUrl($"/register?token={token}"),
            ExpiresAt = invitation.ExpiresAt.ToString(EmailDateFormat, CultureInfo.InvariantCulture)
        });

        await _invitations.SaveChangesAsync(cancellationToken);

        return invitation.ToDto(now);
    }

    // Une invitation en attente encore valable bloque la nouvelle. Une invitation dépassée que la tâche
    // d'expiration n'a pas encore traitée passe à Expired dans la même sauvegarde que la nouvelle.
    private async Task ExpireOutdatedPendingInvitationsAsync(string email, DateTime now, CancellationToken cancellationToken)
    {
        var pendingInvitations = await _invitations.GetPendingByEmailAsync(email, cancellationToken);

        if (pendingInvitations.Any(i => i.IsAwaitingAcceptance(now)))
            throw new ConflictException("Une invitation est déjà en attente pour cet email.");

        foreach (var outdated in pendingInvitations)
            outdated.MarkExpired();
    }
}
