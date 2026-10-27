namespace JewelryShop.Features.Tags.Entities;

/// <summary>Association d'un tag à un article (chaîne ou breloque).</summary>
public interface ITagLink
{
    long TagId { get; }
    Tag Tag { get; }
}
