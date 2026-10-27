using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Tags.Common;
using MediatR;

namespace JewelryShop.Features.Tags.Commands;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, bool>
{
    private readonly ITagRepository _tags;

    public DeleteTagCommandHandler(ITagRepository tags) => _tags = tags;

    // Les liens vers les chaînes et les breloques sont supprimés par la base (ON DELETE CASCADE) ;
    // les articles, eux, restent.
    public async Task<bool> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tags.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(TagMessages.EntityName, request.Id);

        _tags.Delete(tag);
        await _tags.SaveChangesAsync(cancellationToken);

        return true;
    }
}
