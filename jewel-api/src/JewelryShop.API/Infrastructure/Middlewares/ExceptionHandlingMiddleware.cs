using System.Net;
using System.Text.Json;
using JewelryShop.Application.Common.Exceptions;
using ValidationException = JewelryShop.Application.Common.Exceptions.ValidationException;

namespace JewelryShop.Infrastructure.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, _, _) = Map(ex);

            // Les exceptions métier attendues (4xx) sont une réponse normale de l'API,
            // pas un bug serveur : on les journalise sobrement, sans stacktrace.
            // Seules les vraies erreurs imprévues (5xx) gardent le LogError complet.
            if ((int)statusCode >= 500)
                _logger.LogError(ex, "Exception non gérée : {Message}", ex.Message);
            else
                _logger.LogInformation("Requête refusée ({Status}) : {Message}",
                    (int)statusCode, ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static (HttpStatusCode StatusCode, string Title, IReadOnlyDictionary<string, string[]>? Errors) Map(
        Exception exception) =>
        exception switch
        {
            ValidationException ve      => (HttpStatusCode.BadRequest,        "Erreur de validation",   ve.Errors),
            NotFoundException nfe       => (HttpStatusCode.NotFound,          nfe.Message,               null),
            ConflictException ce        => (HttpStatusCode.Conflict,          ce.Message,                null),
            ForbiddenAccessException fe => (HttpStatusCode.Forbidden,         fe.Message,                null),
            UnauthorizedException ue    => (HttpStatusCode.Unauthorized,      ue.Message,                null),
            _                          => (HttpStatusCode.InternalServerError, "Une erreur est survenue.", null)
        };

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = Map(exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            title,
            errors
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
