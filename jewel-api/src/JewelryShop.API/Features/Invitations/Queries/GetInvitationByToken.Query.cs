using JewelryShop.Features.Invitations.DTOs;
using MediatR;

namespace JewelryShop.Features.Invitations.Queries;

public record GetInvitationByTokenQuery(string Token) : IRequest<InvitationPreviewDto>;
