namespace JewelryShop.Features.Invitations.DTOs;

/// <summary>Ce que la page d'inscription affiche d'une invitation : jamais son statut ni l'admin qui l'a envoyée.</summary>
public record InvitationPreviewDto(string Email, DateTime ExpiresAt);

public static class InvitationPreviewMappingExtensions
{
    public static InvitationPreviewDto ToPreviewDto(this Invitation i) => new(i.Email, i.ExpiresAt);
}
