using JewelryShop.Features.Charms.DTOs;
using JewelryShop.Features.Tags.Common;
using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public class CreateCharmCommandHandler : IRequestHandler<CreateCharmCommand, AdminCharmDto>
{
    private readonly ICharmRepository _charms;
    private readonly ITagRepository _tags;

    public CreateCharmCommandHandler(ICharmRepository charms, ITagRepository tags)
    {
        _charms = charms;
        _tags = tags;
    }

    public async Task<AdminCharmDto> Handle(CreateCharmCommand request, CancellationToken cancellationToken)
    {
        var charm = new Charm
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Color = request.Color,
            Cost = request.Cost,
            Price = request.Price,
            Stock = request.Stock,
            Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim()
        };

        await _tags.ApplyToProductAsync(request.TagIds, charm.ReplaceTags, cancellationToken);

        await _charms.AddAsync(charm, cancellationToken);
        await _charms.SaveChangesOrTagNotFoundAsync(cancellationToken);

        return charm.ToAdminDtoWithTags();
    }
}
