using JewelryShop.Application.Common.Exceptions;
using MediatR;

namespace JewelryShop.Application.Features.Charms.Commands;

public record DeleteCharmCommand(long Id) : IRequest<bool>;

public class DeleteCharmCommandHandler : IRequestHandler<DeleteCharmCommand, bool>
{
    private readonly ICharmRepository _charms;

    public DeleteCharmCommandHandler(ICharmRepository charms) => _charms = charms;

    public async Task<bool> Handle(DeleteCharmCommand request, CancellationToken cancellationToken)
    {
        var charm = await _charms.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Breloque", request.Id);

        _charms.Delete(charm);
        await _charms.SaveChangesAsync(cancellationToken);

        return true;
    }
}
