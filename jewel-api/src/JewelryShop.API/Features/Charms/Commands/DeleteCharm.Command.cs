using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public record DeleteCharmCommand(long Id) : IRequest<bool>;
