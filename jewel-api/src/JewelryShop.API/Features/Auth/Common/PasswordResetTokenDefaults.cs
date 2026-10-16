namespace JewelryShop.Features.Auth.Common;

public static class PasswordResetTokenDefaults
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    public const string InvalidTokenMessage = "Ce lien de réinitialisation est invalide ou a expiré.";
}
