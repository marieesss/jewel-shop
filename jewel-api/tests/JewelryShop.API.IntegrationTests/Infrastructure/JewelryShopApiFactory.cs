using Coravel.Invocable;
using JewelryShop.API.IntegrationTests.Fakes;
using JewelryShop.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace JewelryShop.API.IntegrationTests.Infrastructure;

/// <summary>
/// API complète (pipeline HTTP, middlewares, EF) branchée sur un PostgreSQL jetable lancé par Testcontainers.
/// Le schéma est créé par les migrations Evolve jouées au démarrage de l'API, comme en local.
/// </summary>
public class JewelryShopApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Même version majeure que la base de compose.yaml et des manifests Kubernetes.
    private const string PostgresImage = "postgres:16-alpine";

    // Le cookie du refresh token est Secure : le client de test doit parler en HTTPS pour le recevoir et le renvoyer.
    private static readonly Uri _httpsBaseAddress = new("https://localhost");

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder(PostgresImage)
        .Build();

    // L'hôte de test tourne en Development : l'API crée ce compte admin au démarrage, comme en local.
    public const string DevAdminEmail = "admin-dev@integration.test";
    public const string DevAdminPassword = "AdminDev123!";

    public FakeEmailSender EmailSender { get; } = new();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        // Démarre l'hôte (et donc les migrations) une seule fois, avant le premier test.
        _ = Server;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _database.DisposeAsync();
    }

    /// <summary>
    /// Client sans gestion automatique des cookies : chaque test lit le Set-Cookie et choisit le cookie qu'il renvoie
    /// (par exemple un refresh token déjà tourné).
    /// </summary>
    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = _httpsBaseAddress,
        HandleCookies = false,
        AllowAutoRedirect = false
    });

    public async Task SeedAsync(Func<IServiceProvider, Task> seed)
    {
        await using var scope = Services.CreateAsyncScope();
        await seed(scope.ServiceProvider);
    }

    public async Task<T> QueryAsync<T>(Func<IServiceProvider, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        return await query(scope.ServiceProvider);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _database.GetConnectionString());
        builder.UseSetting("Jwt:Secret", "cle-de-signature-des-tests-d-integration-0123456789");
        builder.UseSetting("Jwt:Issuer", "JewelryShop");
        builder.UseSetting("Jwt:Audience", "JewelryShop");
        builder.UseSetting("Jwt:AccessTokenMinutes", "15");
        builder.UseSetting("Email:Enabled", "false");
        builder.UseSetting("DevAdmin:Email", DevAdminEmail);
        builder.UseSetting("DevAdmin:Password", DevAdminPassword);

        builder.ConfigureTestServices(services =>
        {
            RemovePeriodicJobsHost(services);
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    // Sans l'hôte de Coravel, les tâches restent planifiées mais ne s'exécutent jamais :
    // aucune purge ni envoi de mail ne vient modifier la base pendant un test.
    private static void RemovePeriodicJobsHost(IServiceCollection services)
    {
        var coravelAssembly = typeof(IInvocable).Assembly;
        var coravelHostedServices = services
            .Where(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType?.Assembly == coravelAssembly)
            .ToList();

        // Échoue plutôt que de laisser tourner les tâches si Coravel change sa façon d'enregistrer son hôte.
        if (coravelHostedServices.Count == 0)
            throw new InvalidOperationException("Hôte des tâches périodiques de Coravel introuvable.");

        foreach (var descriptor in coravelHostedServices)
            services.Remove(descriptor);
    }
}
