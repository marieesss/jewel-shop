using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Queries;

public record GetTagsQuery : IRequest<IReadOnlyList<TagDto>>;
