using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Queries;

public record GetAdminTagsQuery : IRequest<IReadOnlyList<AdminTagDto>>;
