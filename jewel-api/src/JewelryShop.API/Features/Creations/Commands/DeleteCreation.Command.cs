using MediatR;

namespace JewelryShop.Features.Creations.Commands;

public record DeleteCreationCommand(long UserId, long CreationId) : IRequest<bool>;
