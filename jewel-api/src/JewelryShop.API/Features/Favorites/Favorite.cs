namespace JewelryShop.Application.Features.Favorites;

public class Favorite : BaseEntity
{
    public long UserId { get; set; }
    public long? ChainId { get; set; }
    public long? CharmId { get; set; }

    public User User { get; set; } = null!;
    public Chain? Chain { get; set; }
    public Charm? Charm { get; set; }

    public bool IsValid => ChainId.HasValue ^ CharmId.HasValue;
}
