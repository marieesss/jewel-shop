using System.Globalization;
using JewelryShop.Common.Interfaces;
using JewelryShop.Infrastructure.Email;

namespace JewelryShop.Features.Invitations.Common;

/// <summary>Mail d'invitation, partagé par la création et le renvoi.</summary>
public static class InvitationEmail
{
    private const string DateFormat = "dd/MM/yyyy";

    /// <summary>Met en file le mail avec le lien d'inscription du token en clair, sans sauvegarder.</summary>
    public static void Enqueue(IEmailQueue emailQueue, AppOptions appOptions, Invitation invitation, string token)
        => emailQueue.Enqueue(invitation.Email, EmailTemplates.Invitation, new
        {
            invitation.InviterName,
            RegisterUrl = appOptions.FrontendUrl($"/register?token={token}"),
            ExpiresAt = invitation.ExpiresAt.ToString(DateFormat, CultureInfo.InvariantCulture)
        });
}
