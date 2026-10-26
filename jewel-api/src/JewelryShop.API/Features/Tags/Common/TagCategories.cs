namespace JewelryShop.Features.Tags.Common;

public static class TagCategories
{
    /// <summary>Vrai si la valeur est exactement le nom d'une catégorie (« Theme »), pas un nombre ni une autre casse.</summary>
    public static bool IsValid(string? value) => value is not null && Enum.GetNames<TagCategory>().Contains(value);

    public static TagCategory Parse(string value) => Enum.Parse<TagCategory>(value);
}
