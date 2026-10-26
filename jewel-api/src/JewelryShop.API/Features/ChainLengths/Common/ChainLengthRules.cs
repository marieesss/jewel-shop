namespace JewelryShop.Features.ChainLengths.Common;

/// <summary>Bornes d'une longueur de chaîne, identiques à la contrainte CHECK de la table chain_lengths.</summary>
public static class ChainLengthRules
{
    public const int MinLengthCm = 10;
    public const int MaxLengthCm = 200;
    public const int LabelMaxLength = 50;
}
