namespace JewelryShop.Features.Chains.Entities;

/// <summary>
/// Rouleau de chaîne acheté d'un seul tenant. Une longueur commandée est prise dans un seul rouleau,
/// car deux bouts de chaîne ne s'assemblent pas.
/// </summary>
public class ChainRoll : BaseEntity
{
    public long ChainId { get; private set; }
    public int InitialLengthCm { get; private set; }
    public int RemainingLengthCm { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static ChainRoll Create(int lengthCm, DateTime now) => new()
    {
        InitialLengthCm = lengthCm,
        RemainingLengthCm = lengthCm,
        CreatedAt = now
    };
}
