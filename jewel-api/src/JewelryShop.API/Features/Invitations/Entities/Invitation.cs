namespace JewelryShop.Features.Invitations.Entities;

/// <summary>Invitation à créer un compte, envoyée par un admin. Le token est stocké haché.</summary>
public class Invitation : BaseEntity
{
    public string Email { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public InvitationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public long? InvitedById { get; private set; }
    public User? InvitedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public long? AcceptedUserId { get; private set; }
    public User? AcceptedUser { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public static Invitation Create(string email, string tokenHash, User invitedBy, DateTime now) => new()
    {
        Email = email,
        TokenHash = tokenHash,
        Status = InvitationStatus.Pending,
        ExpiresAt = now + InvitationDefaults.Lifetime,
        InvitedById = invitedBy.Id,
        InvitedBy = invitedBy,
        CreatedAt = now
    };

    /// <summary>
    /// Fait foi partout : une invitation en attente dont l'échéance est atteinte est expirée,
    /// même si la tâche d'expiration n'a pas encore mis son statut à jour.
    /// </summary>
    public bool IsExpired(DateTime now)
        => Status == InvitationStatus.Expired
        || (Status == InvitationStatus.Pending && ExpiresAt <= now);

    public bool IsAwaitingAcceptance(DateTime now) => Status == InvitationStatus.Pending && !IsExpired(now);

    public InvitationStatus EffectiveStatus(DateTime now) => IsExpired(now) ? InvitationStatus.Expired : Status;

    /// <summary>« Prénom Nom » de l'admin, null s'il a été supprimé ou n'a pas de nom.</summary>
    public string? InviterName
    {
        get
        {
            var fullName = $"{InvitedBy?.Name} {InvitedBy?.Surname}".Trim();
            return fullName.Length == 0 ? null : fullName;
        }
    }

    public void MarkExpired() => Status = InvitationStatus.Expired;

    /// <summary>
    /// Passe par la navigation et non par l'id : le compte créé à l'inscription n'a pas encore d'id,
    /// EF renseigne accepted_user_id à la sauvegarde.
    /// </summary>
    public void Accept(User user, DateTime now)
    {
        Status = InvitationStatus.Accepted;
        AcceptedAt = now;
        AcceptedUser = user;
    }
}
