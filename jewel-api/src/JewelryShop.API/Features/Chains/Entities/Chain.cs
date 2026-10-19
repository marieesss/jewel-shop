using JewelryShop.Features.Chains.Common;

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

    /// <summary>
    /// Enregistre les prix saisis. Au nouveau format, les anciennes colonnes ne sont pas touchées ;
    /// à l'ancien, les prix au mètre en sont déduits. Un fermoir absent garde sa valeur.
    /// </summary>
    public void ApplyPrices(IChainPrices prices)
    {
        if (prices.UsesMeterPrices())
        {
            CostPerMeter = prices.CostPerMeter!.Value;
            PricePerMeter = prices.PricePerMeter!.Value;
        }
        else
        {
            Cost = prices.Cost!.Value;
            Price = prices.Price!.Value;
            Length = prices.Length!.Value;
            CostPerMeter = ChainPricing.PerMeter(Cost.Value, Length.Value);
            PricePerMeter = ChainPricing.PerMeter(Price.Value, Length.Value);
        }

        if (prices.ClaspCost is { } claspCost)
            ClaspCost = claspCost;
        if (prices.ClaspPrice is { } claspPrice)
            ClaspPrice = claspPrice;
    }

    public void AddRoll(int lengthCm, DateTime now) => Rolls.Add(ChainRoll.Create(lengthCm, now));
}
