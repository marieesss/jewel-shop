namespace JewelryShop.Features.Users.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateOnly? Birthday { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime? CreatedAt { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }

    public bool HasAddress => AddressLine1 is not null;

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Creation> Creations { get; set; } = new List<Creation>();
}
