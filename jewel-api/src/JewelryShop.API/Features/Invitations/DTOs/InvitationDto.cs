namespace JewelryShop.Features.Invitations.DTOs;

public record InvitationDto(
    long Id,
    string Email,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    string? InvitedBy);

public static class InvitationMappingExtensions
{
    /// <summary>Le statut renvoyé est le statut effectif : Expired dès que l'échéance est atteinte.</summary>
    public static InvitationDto ToDto(this Invitation i, DateTime now) => new(
        i.Id,
        i.Email,
        i.EffectiveStatus(now).ToString(),
        i.ExpiresAt,
        i.CreatedAt,
        i.InviterName);
}
