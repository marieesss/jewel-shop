using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Tags.Common;
using JewelryShop.Features.Tags.DTOs;
using MediatR;

namespace JewelryShop.Features.Tags.Commands;

public class MergeTagCommandHandler : IRequestHandler<MergeTagCommand, TagDto>
{
    private readonly ITagRepository _tags;

    public MergeTagCommandHandler(ITagRepository tags) => _tags = tags;

    public async Task<TagDto> Handle(MergeTagCommand request, CancellationToken cancellationToken)
    {
        if (await _tags.GetByIdAsync(request.Id, cancellationToken) is null)
            throw new NotFoundException(TagMessages.EntityName, request.Id);
        var target = await _tags.GetByIdAsync(request.TargetTagId, cancellationToken)
            ?? throw new NotFoundException(TagMessages.EntityName, request.TargetTagId);

        await _tags.MergeIntoAsync(request.Id, target.Id, cancellationToken);

        return target.ToDto();
    }
}
