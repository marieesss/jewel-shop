using JewelryShop.Features.Charms.DTOs;
using MediatR;

namespace JewelryShop.Features.Charms.Commands;

public class CreateCharmCommandHandler : IRequestHandler<CreateCharmCommand, CharmDto>
{
    private readonly ICharmRepository _charms;

    public CreateCharmCommandHandler(ICharmRepository charms) => _charms = charms;

    public async Task<CharmDto> Handle(CreateCharmCommand request, CancellationToken cancellationToken)
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

        await _charms.AddAsync(charm, cancellationToken);
        await _charms.SaveChangesAsync(cancellationToken);

        return charm.ToDto();
    }
}
