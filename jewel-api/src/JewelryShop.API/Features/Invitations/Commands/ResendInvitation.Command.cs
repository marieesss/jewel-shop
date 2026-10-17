using JewelryShop.Features.Invitations.DTOs;
using MediatR;

namespace JewelryShop.Features.Invitations.Commands;

public record ResendInvitationCommand(long Id) : IRequest<InvitationDto>;
