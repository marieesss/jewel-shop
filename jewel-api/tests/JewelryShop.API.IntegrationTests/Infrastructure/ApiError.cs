namespace JewelryShop.API.IntegrationTests.Infrastructure;

/// <summary>Corps d'erreur écrit par ExceptionHandlingMiddleware.</summary>
public record ApiError(int Status, string Title, Dictionary<string, string[]>? Errors);
