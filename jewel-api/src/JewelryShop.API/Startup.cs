using System.Text;
using FluentValidation;
using JewelryShop.Application.Common.Behaviors;
using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Application.Common.Persistence;
using JewelryShop.Application.Features.Chains;
using JewelryShop.Application.Features.Charms;
using JewelryShop.Application.Features.Creations;
using JewelryShop.Application.Features.Favorites;
using JewelryShop.Application.Features.Options;
using JewelryShop.Application.Features.Users;
using JewelryShop.Infrastructure.Middlewares;
using JewelryShop.Infrastructure.Persistence;
using JewelryShop.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace JewelryShop.API;

/// <summary>
/// Point central d'enregistrement des services (DI) et de la pipeline HTTP.
/// Appelé depuis Program.cs.
/// </summary>
public static class Startup
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        var services      = builder.Services;
        var configuration = builder.Configuration;

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        AddCors(services, configuration);
        AddSwagger(services);
        AddJwtAuthentication(services, configuration);
        AddPersistence(services, configuration);
        AddApplication(services);
        AddInfrastructureServices(services);
    }

    public static void ConfigurePipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        // CORS avant la redirection HTTPS : sinon le preflight OPTIONS reçoit un
        // 307 (que le navigateur ne suit pas sur un preflight) et l'appel échoue.
        app.UseCors(CorsPolicyName);

        // En dev, on n'impose pas le HTTPS : le front appelle http://localhost:5030
        // directement, sans redirection 307 ni problème de certificat auto-signé.
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private const string CorsPolicyName = "FrontendCors";

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        // Origines explicitement autorisées (séparées par des virgules dans Cors__Origins) ;
        // utile en production pour épingler le domaine du front.
        var origins = (configuration["Cors:Origins"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
                policy.SetIsOriginAllowed(origin =>
                      {
                          if (origins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                              return true;

                          // En dev, le port de Vite peut varier (5173, 5174, …) :
                          // on accepte tout localhost / 127.0.0.1, quel que soit le port.
                          return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                              && (uri.Host == "localhost" || uri.Host == "127.0.0.1");
                      })
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "JewelryShop API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Entrez votre JWT : Bearer {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt__Secret manquant dans .env");

        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = configuration["Jwt:Issuer"],
                    ValidAudience            = configuration["Jwt:Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(secret))
                };
            });

        services.AddAuthorization();
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection manquant dans .env");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository,     UserRepository>();
        services.AddScoped<IChainRepository,    ChainRepository>();
        services.AddScoped<ICharmRepository,    CharmRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<ICreationRepository, CreationRepository>();
        services.AddScoped<IOptionRepository,   OptionRepository>();
    }

    private static void AddApplication(IServiceCollection services)
    {
        var assembly = typeof(Startup).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }

    private static void AddInfrastructureServices(IServiceCollection services)
    {
        services.AddScoped<IJwtService,         JwtService>();
        services.AddScoped<IPasswordHasher,     BcryptPasswordHasher>();
        services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
    }
}
