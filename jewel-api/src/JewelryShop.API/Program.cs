using DotNetEnv;
using EvolveDb;
using JewelryShop.API;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envPath))
    envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
Env.Load(envPath, new LoadOptions(setEnvVars: true, clobberExistingVars: false));

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.ConfigureServices();

var app = builder.Build();

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
        EnableClusterMode = false
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
