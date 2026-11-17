using JewelryShop.Common.Enums;
using JewelryShop.Features.Users.Entities;

namespace JewelryShop.API.Tests.Orders;

/// <summary>Comptes utilisés par les tests des commandes côté admin.</summary>
public static class AdminOrderTestData
{
    public const long CustomerId = 4;
    public const long OtherCustomerId = 5;
    public const long AdminId = 1;
    public const long OtherAdminId = 2;

    public static User Customer() => NewUser(CustomerId, "Marie", "Durand", "marie@example.com", UserRole.User);

    public static User OtherCustomer() => NewUser(OtherCustomerId, "Paul", "Leroy", "paul@example.com", UserRole.User);

    public static User Admin() => NewUser(AdminId, "Alice", "Martin", "alice@example.com", UserRole.Admin);

    public static User OtherAdmin() => NewUser(OtherAdminId, "Bruno", "Petit", "bruno@example.com", UserRole.Admin);

    private static User NewUser(long id, string name, string surname, string email, UserRole role) => new()
    {
        Id = id,
        Name = name,
        Surname = surname,
        Email = email,
        PasswordHash = "hash",
        Role = role
    };
}
