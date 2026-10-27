using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public class UpdateCharmCommandHandler : IRequestHandler<UpdateCharmCommand, AdminCharmDto>
{
    private readonly ICharmRepository _charms;

    public UpdateCharmCommandHandler(ICharmRepository charms) => _charms = charms;

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

        _charms.Update(charm);
        await _charms.SaveChangesAsync(cancellationToken);

        return charm.ToAdminDtoWithTags();
    }
}
