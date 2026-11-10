using JewelryShop.API.Tests.Fakes;
using JewelryShop.Common.Enums;
using JewelryShop.Features.Auth.Seeding;
using JewelryShop.Features.Users.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JewelryShop.API.Tests.Auth;

public class DevAdminSeederTests
{
    private const string Email = "admin@mycharmly.test";
    private const string Password = "MotDePasse123!";

    private static readonly DateTimeOffset _now = new(2026, 10, 28, 9, 30, 0, TimeSpan.Zero);

    private readonly FakeUserRepository _users = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly ListLogger<DevAdminSeeder> _logger = new();

    private DevAdminSeeder CreateSeeder(string environmentName = "Development", string email = Email, string password = Password)
        => new(
            _users,
            _passwordHasher,
            new FakeHostEnvironment(environmentName),
            Options.Create(new DevAdminOptions { Email = email, Password = password }),
            new FakeTimeProvider(_now),
            _logger);

    [Fact]
    public async Task Cree_un_admin_quand_aucun_compte_n_a_cet_email()
    {
        await CreateSeeder().SeedAsync();

        var user = Assert.Single(_users.All);
        Assert.Equal(Email, user.Email);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Equal(DevAdminSeeder.AdminName, user.Name);
        Assert.Equal(DevAdminSeeder.AdminSurname, user.Surname);
        Assert.Equal(_now.UtcDateTime, user.CreatedAt);
        Assert.Equal(1, _users.SaveChangesCount);
    }

    [Fact]
    public async Task Hache_le_mot_de_passe_de_l_admin()
    {
        await CreateSeeder().SeedAsync();

        var user = Assert.Single(_users.All);
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.True(_passwordHasher.Verify(Password, user.PasswordHash));
    }

    [Fact]
    public async Task Normalise_l_email_comme_a_la_connexion()
    {
        await CreateSeeder(email: "  Admin@MyCharmly.test ").SeedAsync();

        Assert.Equal(Email, Assert.Single(_users.All).Email);
    }

    [Fact]
    public async Task Journalise_la_creation_du_compte()
    {
        await CreateSeeder().SeedAsync();

        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal($"Compte admin de développement créé : {Email}", entry.Message);
    }

    [Fact]
    public async Task Ne_modifie_pas_un_compte_existant_avec_cet_email()
    {
        var existing = new User
        {
            Name = "Marie",
            Surname = "Existante",
            Email = Email,
            PasswordHash = _passwordHasher.Hash("AncienMotDePasse1!"),
            Role = UserRole.User
        };
        _users.Seed(existing);

        await CreateSeeder().SeedAsync();

        var user = Assert.Single(_users.All);
        Assert.Same(existing, user);
        Assert.Equal(UserRole.User, user.Role);
        Assert.True(_passwordHasher.Verify("AncienMotDePasse1!", user.PasswordHash));
        Assert.Equal(0, _users.SaveChangesCount);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task Ne_cree_qu_un_seul_compte_sur_plusieurs_demarrages()
    {
        await CreateSeeder().SeedAsync();
        await CreateSeeder().SeedAsync();

        Assert.Single(_users.All);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("", Password)]
    [InlineData(Email, "")]
    [InlineData("   ", "   ")]
    public async Task Ne_cree_aucun_compte_si_une_variable_est_vide(string email, string password)
    {
        await CreateSeeder(email: email, password: password).SeedAsync();

        Assert.Empty(_users.All);
        Assert.Equal(0, _users.SaveChangesCount);
        Assert.Empty(_logger.Entries);
    }

    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Ne_cree_aucun_compte_hors_developpement(string environmentName)
    {
        await CreateSeeder(environmentName).SeedAsync();

        Assert.Empty(_users.All);
        Assert.Equal(0, _users.SaveChangesCount);
        Assert.Empty(_logger.Entries);
    }
}
