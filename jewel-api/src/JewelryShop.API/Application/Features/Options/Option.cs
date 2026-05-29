namespace JewelryShop.Application.Features.Options;

public class Option : BaseEntity
{
    public string Name { get; set; } = null!;
    public decimal Length { get; set; }

    public ICollection<Creation> Creations { get; set; } = new List<Creation>();
}
