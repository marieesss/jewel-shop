namespace JewelryShop.Features.Users.Common;

/// <summary>Règles de l'adresse de livraison (France uniquement) et du téléphone, alignées sur les colonnes de users.</summary>
public static class UserAddressRules
{
    public const int AddressLineMaxLength = 255;
    public const int CityMaxLength = 100;
    public const int PostalCodeLength = 5;
    public const int PhoneMaxLength = 20;

    // [0-9] et \z : \d accepte les chiffres Unicode et $ un saut de ligne final.
    public const string PostalCodePattern = @"^[0-9]{5}\z";

    // Numéro français : 10 chiffres commençant par 0, ou +33 suivi des 9 chiffres sans le 0.
    public const string PhonePattern = @"^(0|\+33)[1-9][0-9]{8}\z";
}
