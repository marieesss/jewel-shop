using JewelryShop.Common;
using JewelryShop.Features.Invitations.DTOs;
using MediatR;

namespace JewelryShop.Features.Invitations.Queries;

public record GetInvitationsQuery(string? Status, int Page = 1, int PageSize = 20) : IRequest<PagedResult<InvitationDto>>;
