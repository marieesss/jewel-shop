using JewelryShop.Features.Chains.Common;
using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Commands;

public record CreateChainCommand(
    string Name,
    string Description,
    ProductColor Color,
    decimal? Cost = null,
    decimal? Price = null,
    decimal? Length = null,
    string? Url = null,
    decimal? CostPerMeter = null,
    decimal? PricePerMeter = null,
    decimal? ClaspCost = null,
    decimal? ClaspPrice = null,
    int? InitialRollLengthCm = null,
    long[]? TagIds = null) : IRequest<AdminChainDto>, IChainPrices;
