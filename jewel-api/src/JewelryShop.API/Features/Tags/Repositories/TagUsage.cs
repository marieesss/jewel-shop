namespace JewelryShop.Features.Tags.Repositories;

/// <summary>Tag avec le nombre de chaînes et de breloques qui le portent.</summary>
public record TagUsage(Tag Tag, int ChainCount, int CharmCount);
