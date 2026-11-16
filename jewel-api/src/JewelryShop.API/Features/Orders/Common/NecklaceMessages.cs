namespace JewelryShop.Features.Orders.Common;

public static class NecklaceMessages
{
    public const string CharmRequired = "Ajoutez au moins une breloque.";
    public const string TooManyCharms = "8 breloques maximum.";
    public const string InvalidId = "Identifiant invalide.";
    public const string ChainNotFound = "Chaîne introuvable.";
    public const string CharmNotFound = "Breloque introuvable.";
    public const string LengthNotOffered = "Longueur non proposée.";

    public static string OutOfStock(string productName) =>
        $"Le stock de « {productName} » ne permet plus cette commande.";
}
