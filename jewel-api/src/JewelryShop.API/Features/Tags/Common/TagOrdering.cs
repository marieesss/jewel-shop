namespace JewelryShop.Features.Tags.Common;

public static class TagOrdering
{
    public static IEnumerable<T> OrderByCategoryThenName<T>(this IEnumerable<T> items, Func<T, Tag> tag)
        => items.OrderBy(i => tag(i).Category).ThenBy(i => tag(i).Name, StringComparer.CurrentCultureIgnoreCase);
}
