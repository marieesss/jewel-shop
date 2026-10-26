namespace JewelryShop.Features.ChainLengths.DTOs;

public record ChainLengthDto(long Id, int LengthCm, string Label);

public static class ChainLengthMappingExtensions
{
    public static ChainLengthDto ToDto(this ChainLength length) => new(length.Id, length.LengthCm, length.Label);
}
