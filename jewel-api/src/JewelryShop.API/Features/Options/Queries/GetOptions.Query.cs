using JewelryShop.Features.Options.DTOs;
using MediatR;

namespace JewelryShop.Features.Options.Queries;

public record GetOptionsQuery : IRequest<IEnumerable<OptionDto>>;
