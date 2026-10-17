namespace JewelryShop.Features.Invitations.Common;

public static class InvitationDefaults
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    public const string InvalidInvitationMessage = "Cette invitation est invalide ou a expiré.";
}
