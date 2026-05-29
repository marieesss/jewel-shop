using System.Text;
using FluentValidation;
using JewelryShop.Application.Common.Behaviors;
using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Application.Common.Persistence;
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

        AddSwagger(services);
        AddJwtAuthentication(services, configuration);
        AddPersistence(services, configuration);
        AddApplication(services);
        AddInfrastructureServices(services);
    }

    public static void ConfigurePipeline(this WebApplication app)
    {
        // Doit être en premier pour intercepter toutes les exceptions
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();   // sert wwwroot/uploads/

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "JewelryShop API", Version = "v1" });

            // Bouton Authorize dans Swagger UI
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
            ?? throw new InvalidOperationException("Jwt:Secret manquant dans la configuration.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
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
                "Connection string 'DefaultConnection' introuvable dans la configuration.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChainRepository, ChainRepository>();
        services.AddScoped<ICharmRepository, CharmRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<ICreationRepository, CreationRepository>();
        services.AddScoped<IOptionRepository, OptionRepository>();
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
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
    }
}
