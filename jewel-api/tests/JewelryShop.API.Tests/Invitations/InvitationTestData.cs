using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Users.Entities;

namespace JewelryShop.API.Tests.Invitations;

/// <summary>
/// Construit des invitations dans un état donné. Aucune méthode de l'entité ne mène encore à Accepted :
/// le statut et les dates sont posés par réflexion sur leurs setters privés.
/// </summary>
public static class InvitationTestData
{
    public static readonly User Admin = new() { Id = 1, Name = "Marie", Surname = "Espinosa", Email = "marie@mycharmly.eu" };

    public static Invitation Create(
        long id,
        string email,
        DateTime createdAt,
        InvitationStatus status = InvitationStatus.Pending,
        DateTime? revokedAt = null)
    {
        var invitation = Invitation.Create(email, new string('a', 64), Admin, createdAt);
        invitation.Id = id;
        Set(invitation, nameof(Invitation.Status), status);
        Set(invitation, nameof(Invitation.RevokedAt), revokedAt);
        return invitation;
    }

    private static void Set(Invitation invitation, string property, object? value)
        => typeof(Invitation).GetProperty(property)!.SetValue(invitation, value);
}
