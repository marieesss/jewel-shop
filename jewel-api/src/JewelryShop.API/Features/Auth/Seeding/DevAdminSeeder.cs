using JewelryShop.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace JewelryShop.Features.Auth.Seeding;

/// <summary>
/// Crée un compte admin en développement, pour qu'une base neuve soit utilisable sans invitation
/// (seul un admin peut inviter). Un compte existant avec cet email n'est jamais modifié.
/// </summary>
public class DevAdminSeeder
{
    public const string AdminName = "Admin";
    public const string AdminSurname = "Local";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IHostEnvironment _environment;
    private readonly DevAdminOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DevAdminSeeder> _logger;

    public DevAdminSeeder(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IHostEnvironment environment,
        IOptions<DevAdminOptions> options,
        TimeProvider timeProvider,
        ILogger<DevAdminSeeder> logger)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _environment = environment;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment() || !_options.IsConfigured)
            return;

        var email = _options.Email.Trim().ToLowerInvariant();
        if (await _users.ExistsByEmailAsync(email, cancellationToken))
            return;

        await _users.AddAsync(new User
        {
            Name = AdminName,
            Surname = AdminSurname,
            Email = email,
            PasswordHash = _passwordHasher.Hash(_options.Password),
            Role = UserRole.Admin,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        }, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Compte admin de développement créé : {Email}", email);
    }
}
