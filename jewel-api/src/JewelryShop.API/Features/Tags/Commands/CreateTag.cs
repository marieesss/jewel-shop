using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Commands;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly ITagRepository _tags;

    public CreateTagCommandHandler(ITagRepository tags) => _tags = tags;

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = new Tag(request.Name, TagCategories.Parse(request.Category));

        await _tags.EnsureNameIsAvailableAsync(tag.Category, tag.Name, null, cancellationToken);

        await _tags.AddAsync(tag, cancellationToken);
        await _tags.SaveChangesOrConflictAsync(tag.Name, cancellationToken);

        return tag.ToDto();
    }
}
