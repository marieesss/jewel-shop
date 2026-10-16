using JewelryShop.Features.Creations.DTOs;
using MediatR;

namespace JewelryShop.Features.Creations.Queries;

public record GetCreationByIdQuery(long UserId, long CreationId) : IRequest<CreationDto>;
