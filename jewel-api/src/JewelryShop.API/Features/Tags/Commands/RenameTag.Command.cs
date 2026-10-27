using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Commands;

/// <summary>Corrige le nom d'un tag ; son slug est recalculé. La catégorie ne change pas.</summary>
public record RenameTagCommand(long Id, string Name) : IRequest<TagDto>;
