using JewelryShop.Features.Invitations.DTOs;
using MediatR;

namespace JewelryShop.Features.Invitations.Commands;

public record CreateInvitationCommand(string Email, long InvitedById) : IRequest<InvitationDto>;
