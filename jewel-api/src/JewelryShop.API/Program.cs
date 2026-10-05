using DotNetEnv;
using EvolveDb;
using JewelryShop.API;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envPath))
    envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
// En conteneur, il n'y a pas de .env : la configuration arrive par les variables d'environnement.
if (File.Exists(envPath))
    Env.Load(envPath, new LoadOptions(setEnvVars: true, clobberExistingVars: false));

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.ConfigureServices();

var app = builder.Build();

// "dotnet JewelryShop.API.dll --migrate" : applique les migrations puis s'arrête,
// sans démarrer le serveur web. Utilisé par le déploiement avant de remplacer l'API.
if (args.Contains("--migrate"))
{
    try
    {
        RunMigrations(app);
    }
    catch
    {
        // Déjà journalisé par RunMigrations : on sort proprement avec un code d'erreur,
        // que le script de déploiement détecte pour s'arrêter.
        Environment.ExitCode = 1;
    }
    return;
}

// En dev, pratique de migrer au démarrage ; en prod, le déploiement s'en charge
// (Database__MigrateOnStartup=false dans compose.prod.yaml).
if (app.Configuration.GetValue("Database:MigrateOnStartup", defaultValue: true))
    RunMigrations(app);

app.ConfigurePipeline();
app.Run();


static void RunMigrations(WebApplication app)
{
    var logger           = app.Logger;
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection introuvable.");

    // Résolution du dossier db/migrations (racine de la solution)
    var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "db", "migrations");
    if (!Directory.Exists(migrationsPath))
        migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "db", "migrations");

    using var cnx = new Npgsql.NpgsqlConnection(connectionString);

    var evolve = new Evolve(cnx, msg => logger.LogInformation("[Evolve] {Msg}", msg))
    {
        Locations        = new[] { migrationsPath },
        IsEraseDisabled  = true,   // Interdit DROP en production
        // Verrou Postgres (advisory lock) : si deux instances migrent en même temps
        // (plusieurs réplicas sous Kubernetes), la seconde attend la première.
        EnableClusterMode = true
    };

    try
    {
        evolve.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "[Evolve] Migration échouée — l'application ne peut pas démarrer.");
        throw;
    }
}
