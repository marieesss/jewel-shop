using JewelryShop.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Features.Charms.Queries;

public record GetCharmByIdQuery(long Id, bool IncludeAdminFields = false) : IRequest<CharmDto>;
