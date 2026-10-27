using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Charms.DTOs;
using JewelryShop.Features.Tags.Common;
using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public class UpdateCharmCommandHandler : IRequestHandler<UpdateCharmCommand, AdminCharmDto>
{
    private readonly ICharmRepository _charms;
    private readonly ITagRepository _tags;

    public UpdateCharmCommandHandler(ICharmRepository charms, ITagRepository tags)
    {
        _charms = charms;
        _tags = tags;
    }

    public async Task<AdminCharmDto> Handle(UpdateCharmCommand request, CancellationToken cancellationToken)
    {
        var charm = await _charms.GetByIdWithTagsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Breloque", request.Id);

        charm.Name = request.Name.Trim();
        charm.Description = request.Description?.Trim() ?? string.Empty;
        charm.Color = request.Color;
        charm.Cost = request.Cost;
        charm.Price = request.Price;
        charm.Stock = request.Stock;
        charm.Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim();

        // Le front en production n'envoie pas tagIds : son absence laisse les tags tels quels.
        if (request.TagIds is { } tagIds)
            charm.ReplaceTags(await _tags.GetForProductAsync(tagIds, nameof(request.TagIds), cancellationToken));

        _charms.Update(charm);
        await _charms.SaveChangesAsync(cancellationToken);

        return charm.ToAdminDtoWithTags();
    }
}
