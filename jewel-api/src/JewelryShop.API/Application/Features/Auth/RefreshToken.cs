namespace JewelryShop.Application.Features.Auth;

/// <summary>
/// Refresh token opaque, stocké haché. Un token tourné ou révoqué est gardé jusqu'à son expiration
/// pour détecter sa réutilisation.
/// </summary>
public class RefreshToken : BaseEntity
{
    public long UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RotatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public static RefreshToken Create(User user, string tokenHash, DateTime now) => new()
    {
        UserId = user.Id,
        User = user,
        TokenHash = tokenHash,
        CreatedAt = now,
        ExpiresAt = now + RefreshTokenDefaults.Lifetime
    };

    public bool IsRotated => RotatedAt.HasValue;

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired(DateTime now) => ExpiresAt <= now;

    public bool IsActive(DateTime now) => !IsRotated && !IsRevoked && !IsExpired(now);

    public void MarkRotated(DateTime now) => RotatedAt = now;

    public void Revoke(DateTime now) => RevokedAt = now;
}
