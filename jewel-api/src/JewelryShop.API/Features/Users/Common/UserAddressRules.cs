namespace JewelryShop.Features.Users.Common;

/// <summary>Règles de l'adresse de livraison (France uniquement) et du téléphone, alignées sur les colonnes de users.</summary>
public static class UserAddressRules
{
    public const int AddressLineMaxLength = 255;
    public const int CityMaxLength = 100;
    public const string PostalCodePattern = @"^\d{5}$";

    // Numéro français : 10 chiffres commençant par 0, ou +33 suivi des 9 chiffres sans le 0.
    public const string PhonePattern = @"^(0|\+33)[1-9]\d{8}$";
}
