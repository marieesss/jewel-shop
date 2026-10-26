using JewelryShop.Features.ChainLengths.Common;
using JewelryShop.Features.ChainLengths.DTOs;
using MediatR;

namespace JewelryShop.Features.ChainLengths.Commands;

public record CreateChainLengthCommand(int LengthCm, string Label) : IRequest<ChainLengthDto>, IChainLengthFields;
