
namespace JewelryShop.Application.Features.Options.DTOs;

public record OptionDto(long Id, string Name, decimal Length);

public static class OptionMappingExtensions
{
    public static OptionDto ToDto(this Option o) => new(o.Id, o.Name, o.Length);
}
