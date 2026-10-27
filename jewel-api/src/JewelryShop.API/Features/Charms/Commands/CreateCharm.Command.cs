using JewelryShop.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public record CreateCharmCommand(
    string Name,
    string Description,
    ProductColor Color,
    decimal Cost,
    decimal Price,
    int Stock,
    string? Url = null,
    long[]? TagIds = null) : IRequest<AdminCharmDto>;
