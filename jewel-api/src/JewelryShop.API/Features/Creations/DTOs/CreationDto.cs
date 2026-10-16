using JewelryShop.Features.Chains.DTOs;
using JewelryShop.Features.Charms.DTOs;
using JewelryShop.Features.Options.DTOs;

namespace JewelryShop.Features.Creations.DTOs;

public record CreationItemDto(long Id, long CharmId, int Position, CharmDto? Charm);

public record CreationDto(
    long Id,
    long UserId,
    long ChainId,
    long OptionId,
    DateTime CreatedAt,
    ChainDto? Chain,
    OptionDto? Option,
    IReadOnlyList<CreationItemDto> Items);

public static class CreationMappingExtensions
{
    public static CreationItemDto ToDto(this CreationItem i) =>
        new(i.Id, i.CharmId, i.Position, i.Charm?.ToDto());

    public static CreationDto ToDto(this Creation c) => new(
        c.Id,
        c.UserId,
        c.ChainId,
        c.OptionId,
        c.CreatedAt,
        c.Chain?.ToDto(),
        c.Option?.ToDto(),
        c.Items.OrderBy(i => i.Position).Select(i => i.ToDto()).ToList());
}
