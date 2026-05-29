namespace JewelryShop.Application.Features.Users;

public class User : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateOnly? Birthday { get; set; }
    public UserRole Role { get; set; } = UserRole.User;

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Creation> Creations { get; set; } = new List<Creation>();
}
