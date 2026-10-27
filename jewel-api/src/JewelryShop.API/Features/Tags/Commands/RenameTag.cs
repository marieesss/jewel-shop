using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Commands;

public class RenameTagCommandHandler : IRequestHandler<RenameTagCommand, TagDto>
{
    private readonly ITagRepository _tags;

    public RenameTagCommandHandler(ITagRepository tags) => _tags = tags;

    public async Task<TagDto> Handle(RenameTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tags.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(TagMessages.EntityName, request.Id);

        var name = request.Name.Trim();
        await _tags.EnsureNameIsAvailableAsync(tag.Category, name, tag.Id, cancellationToken);

        tag.Rename(name);

        _tags.Update(tag);
        await _tags.SaveChangesOrConflictAsync(tag.Name, cancellationToken);

        return tag.ToDto();
    }
}
