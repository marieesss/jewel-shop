namespace JewelryShop.Features.Chains.Entities;

public class Chain : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public ProductColor Color { get; set; }
    public string? ImageUrl { get; set; }
    public string? Url { get; set; }
    public decimal? Cost { get; set; }
    public decimal? Price { get; set; }
    public decimal? Length { get; set; }
    public decimal? CostPerMeter { get; set; }
    public decimal? PricePerMeter { get; set; }
    public decimal ClaspCost { get; set; }
    public decimal ClaspPrice { get; set; }

    public ICollection<ChainRoll> Rolls { get; set; } = new List<ChainRoll>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Creation> Creations { get; set; } = new List<Creation>();

    public int StockCm => Rolls.Sum(r => r.RemainingLengthCm);
}
