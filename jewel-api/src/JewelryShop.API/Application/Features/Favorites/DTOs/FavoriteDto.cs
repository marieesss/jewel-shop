using JewelryShop.Application.Features.Chains.DTOs;
using JewelryShop.Application.Features.Charms.DTOs;

namespace JewelryShop.Application.Features.Favorites.DTOs;

public record FavoriteDto(
    long Id,
    long UserId,
    long? ChainId,
    long? CharmId,
    ChainDto? Chain,
    CharmDto? Charm);

public static class FavoriteMappingExtensions
{
    public static FavoriteDto ToDto(this Favorite f) => new(
        f.Id,
        f.UserId,
        f.ChainId,
        f.CharmId,
        f.Chain?.ToDto(),
        f.Charm?.ToDto());
}
