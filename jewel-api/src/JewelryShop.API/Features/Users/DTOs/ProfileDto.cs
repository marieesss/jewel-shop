namespace JewelryShop.Features.Users.DTOs;

/// <summary>Profil de l'utilisateur connecté. <see cref="Address"/> est null tant qu'aucune adresse n'est enregistrée.</summary>
public record ProfileDto(
    long Id,
    string Name,
    string Surname,
    string Email,
    DateOnly? Birthday,
    string? Phone,
    AddressDto? Address);

public record AddressDto(string Line1, string? Line2, string PostalCode, string City);

public static class ProfileMappingExtensions
{
    public static ProfileDto ToProfileDto(this User u) => new(
        u.Id,
        u.Name,
        u.Surname,
        u.Email,
        u.Birthday,
        u.Phone,
        u.ToAddressDto());

    private static AddressDto? ToAddressDto(this User u) =>
        u.HasAddress
            ? new AddressDto(u.AddressLine1!, u.AddressLine2, u.PostalCode!, u.City!)
            : null;
}
