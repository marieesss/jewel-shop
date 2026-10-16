using JewelryShop.Features.Creations.DTOs;
using MediatR;

namespace JewelryShop.Features.Creations.Commands;

public record CreateCreationItemInput(long CharmId, int Position);

public record CreateCreationCommand(
    long UserId,
    long ChainId,
    long OptionId,
    IReadOnlyList<CreateCreationItemInput> Items) : IRequest<CreationDto>;
