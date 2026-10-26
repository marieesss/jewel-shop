namespace JewelryShop.Features.ChainLengths.Common;

/// <summary>Champs saisis à la création ou à la modification d'une longueur de chaîne.</summary>
public interface IChainLengthFields
{
    int LengthCm { get; }
    string Label { get; }
}
