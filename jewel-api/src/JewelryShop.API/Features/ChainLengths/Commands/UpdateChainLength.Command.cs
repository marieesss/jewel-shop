using JewelryShop.Features.ChainLengths.Common;
using JewelryShop.Features.ChainLengths.DTOs;
using MediatR;

namespace JewelryShop.Features.ChainLengths.Commands;

public record UpdateChainLengthCommand(long Id, int LengthCm, string Label) : IRequest<ChainLengthDto>, IChainLengthFields;
