using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Queries;

/// <summary>Tags avec le nombre global d'articles du type <see cref="Kind"/> (« chain » ou « charm ») qui les portent.</summary>
public record GetTagCountsQuery(string Kind) : IRequest<IReadOnlyList<TagCountDto>>;
