using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Queries;

public record GetChainByIdQuery(long Id) : IRequest<ChainDto>;
