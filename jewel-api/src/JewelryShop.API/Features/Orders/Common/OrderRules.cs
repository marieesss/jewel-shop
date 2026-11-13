namespace JewelryShop.Features.Orders.Common;

/// <summary>Tailles des colonnes des tables de commandes.</summary>
public static class OrderRules
{
    public const int NumberMaxLength = 20;
    public const int StatusMaxLength = 20;
    public const int CommentMaxLength = 1000;
    public const int RecipientNameMaxLength = 100;
    public const int RecipientPhoneMaxLength = 20;
    public const int AddressLineMaxLength = 255;
    public const int PostalCodeMaxLength = 5;
    public const int CityMaxLength = 100;
    public const int LineKindMaxLength = 10;
    public const int LineNameMaxLength = 255;
    public const int StatusMessageMaxLength = 1000;
    public const int NoteContentMaxLength = 2000;
}
