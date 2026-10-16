namespace JewelryShop.Features.Auth.Entities;

/// <summary>Token de réinitialisation du mot de passe, à usage unique, stocké haché.</summary>
public class PasswordResetToken : BaseEntity
{
    public long UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public static PasswordResetToken Create(User user, string tokenHash, DateTime now) => new()
    {
        UserId = user.Id,
        User = user,
        TokenHash = tokenHash,
        CreatedAt = now,
        ExpiresAt = now + PasswordResetTokenDefaults.Lifetime
    };

    public bool IsUsed => UsedAt.HasValue;

    public bool IsExpired(DateTime now) => ExpiresAt <= now;

    public bool IsUsable(DateTime now) => !IsUsed && !IsExpired(now);

    public void MarkUsed(DateTime now) => UsedAt = now;
}
