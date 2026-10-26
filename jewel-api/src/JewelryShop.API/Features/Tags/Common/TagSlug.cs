using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace JewelryShop.Features.Tags.Common;

/// <summary>Slug d'un tag : minuscules sans accents, chaque suite d'autres caractères devient un tiret.</summary>
public static partial class TagSlug
{
    public static string From(string name)
    {
        var lowered = name.Trim().ToLowerInvariant()
            .Replace("œ", "oe")
            .Replace("æ", "ae");

        var slug = NonAlphanumeric().Replace(RemoveDiacritics(lowered), "-").Trim('-');

        return slug.Length <= TagRules.SlugMaxLength ? slug : slug[..TagRules.SlugMaxLength].TrimEnd('-');
    }

    public static bool CanBeBuiltFrom(string name) => From(name).Length > 0;

    private static string RemoveDiacritics(string text)
    {
        var withoutMarks = text.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

        return new string(withoutMarks.ToArray()).Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
