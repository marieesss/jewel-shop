namespace JewelryShop.Common;

/// <summary>Configuration générale de l'application (variables App__*).</summary>
public class AppOptions
{
    public const string SectionName = "App";

    public const string DefaultFrontendBaseUrl = "http://localhost:5173";

    /// <summary>URL publique du front, pour construire les liens des mails.</summary>
    public string FrontendBaseUrl { get; set; } = DefaultFrontendBaseUrl;

    /// <summary>Lien absolu vers le front. <paramref name="pathAndQuery"/> commence par « / ».</summary>
    public string FrontendUrl(string pathAndQuery) => FrontendBaseUrl.TrimEnd('/') + pathAndQuery;
}
