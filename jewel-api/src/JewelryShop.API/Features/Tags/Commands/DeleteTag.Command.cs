using MediatR;

namespace JewelryShop.Features.Tags.Commands;

public record DeleteTagCommand(long Id) : IRequest<bool>;
