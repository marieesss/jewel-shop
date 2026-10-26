using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Commands;

/// <summary>La catégorie est attendue sous forme de nom (« Theme », « Color », « Material », « Occasion »).</summary>
public record CreateTagCommand(string Name, string Category) : IRequest<TagDto>;
