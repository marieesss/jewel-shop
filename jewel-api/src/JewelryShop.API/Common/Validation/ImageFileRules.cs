namespace JewelryShop.Common.Validation;

/// <summary>
/// Règles d'un fichier image uploadé (type, extension, taille), partagées par les
/// validateurs d'upload. Reprises côté front dans <c>lib/imageFile.ts</c>.
/// </summary>
public static class ImageFileRules
{
    public const string PropertyName = "File";
    public const int MaxSizeInMegabytes = 5;
    public const long MaxSizeInBytes = MaxSizeInMegabytes * 1024 * 1024;

    public const string ContentTypeMessage = "Le fichier doit être une image JPEG, PNG ou WEBP.";
    public const string ExtensionMessage = "L'extension du fichier ne correspond pas à son format.";
    public static readonly string MaxSizeMessage = $"L'image ne doit pas dépasser {MaxSizeInMegabytes} Mo.";

    private static readonly Dictionary<string, string[]> _extensionsByContentType =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = [".jpg", ".jpeg"],
            ["image/png"] = [".png"],
            ["image/webp"] = [".webp"],
        };

    public static bool IsAllowedContentType(string contentType) =>
        _extensionsByContentType.ContainsKey(contentType);

    public static bool ExtensionMatchesContentType(string fileName, string contentType) =>
        _extensionsByContentType.TryGetValue(contentType, out var extensions)
        && extensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);
}
