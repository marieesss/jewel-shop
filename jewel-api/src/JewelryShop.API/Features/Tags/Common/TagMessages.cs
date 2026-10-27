namespace JewelryShop.Features.Tags.Common;

public static class TagMessages
{
    public const string EntityName = "Tag";
    public const string NameRequired = "Le nom est requis.";
    public const string NameTooLong = "50 caractères maximum.";
    public const string NameWithoutLetterOrDigit = "Le nom doit contenir au moins une lettre ou un chiffre.";
    public const string InvalidCategory = "Catégorie invalide.";
    public const string InvalidId = "Identifiant invalide.";
    public const string CannotMergeWithItself = "Un tag ne peut pas être fusionné avec lui-même.";
    public const string NotFound = "Tag introuvable.";

    public static string AlreadyExists(string name) => $"Le tag « {name} » existe déjà dans cette catégorie.";

    public static string TooCloseTo(string existingName) => $"Un tag au nom trop proche existe déjà dans cette catégorie : « {existingName} ».";
}
