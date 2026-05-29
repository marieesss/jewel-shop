namespace JewelryShop.Application.Features.Chains;

public class Chain : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public ProductColor Color { get; set; }
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal Length { get; set; }

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Creation> Creations { get; set; } = new List<Creation>();
}
