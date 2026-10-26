namespace JewelryShop.Features.ChainLengths.Entities;

/// <summary>Longueur proposée au client pour une chaîne. Une commande copie la longueur, elle ne la référence pas.</summary>
public class ChainLength : BaseEntity
{
    public int LengthCm { get; set; }
    public string Label { get; set; } = null!;
}
