using MediatR;

namespace JewelryShop.Features.Invitations.Commands;

public record RevokeInvitationCommand(long Id) : IRequest;
