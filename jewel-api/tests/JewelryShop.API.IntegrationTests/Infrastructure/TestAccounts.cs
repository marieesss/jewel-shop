using JewelryShop.Common.Enums;
using JewelryShop.Common.Interfaces;
using JewelryShop.Features.Invitations.Entities;
using JewelryShop.Features.Users.Entities;
using JewelryShop.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.IntegrationTests.Infrastructure;

/// <summary>Crée directement en base les comptes et les invitations dont les tests ont besoin.</summary>
public static class TestAccounts
{
    public const string Password = "Motdepasse1!";

    public static string UniqueEmail() => $"{Guid.NewGuid():N}@tests.mycharmly.eu";

    public static async Task<User> CreateUserAsync(this JewelryShopApiFactory api, UserRole role = UserRole.User)
    {
        User user = null!;
        await api.SeedAsync(async services =>
        {
            var db = services.GetRequiredService<AppDbContext>();
            user = NewUser(services, role);
            db.Users.Add(user);
            await db.SaveChangesAsync();
        });
        return user;
    }

    /// <summary>Crée une invitation en attente, envoyée par un admin, et renvoie son email et le token en clair du lien.</summary>
    public static async Task<(string Email, string Token)> CreatePendingInvitationAsync(this JewelryShopApiFactory api)
    {
        var email = UniqueEmail();
        string token = null!;
        await api.SeedAsync(async services =>
        {
            var db = services.GetRequiredService<AppDbContext>();
            var tokenGenerator = services.GetRequiredService<ISecureTokenGenerator>();
            var admin = NewUser(services, UserRole.Admin);
            token = tokenGenerator.Generate();
            db.Users.Add(admin);
            db.Invitations.Add(Invitation.Create(email, tokenGenerator.Hash(token), admin, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });
        return (email, token);
    }

    private static User NewUser(IServiceProvider services, UserRole role) => new()
    {
        Name = "Marie",
        Surname = "Test",
        Email = UniqueEmail(),
        PasswordHash = services.GetRequiredService<IPasswordHasher>().Hash(Password),
        Role = role,
        CreatedAt = DateTime.UtcNow
    };
}
