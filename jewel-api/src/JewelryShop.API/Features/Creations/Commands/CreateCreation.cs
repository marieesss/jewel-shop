using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Creations.DTOs;
using MediatR;

namespace JewelryShop.Features.Creations.Commands;

public class CreateCreationCommandHandler : IRequestHandler<CreateCreationCommand, CreationDto>
{
    private readonly ICreationRepository _creations;
    private readonly IChainRepository _chains;
    private readonly IOptionRepository _options;
    private readonly ICharmRepository _charms;

    public CreateCreationCommandHandler(
        ICreationRepository creations,
        IChainRepository chains,
        IOptionRepository options,
        ICharmRepository charms)
    {
        _creations = creations;
        _chains = chains;
        _options = options;
        _charms = charms;
    }

    public async Task<CreationDto> Handle(CreateCreationCommand request, CancellationToken cancellationToken)
    {
        if (await _chains.GetByIdAsync(request.ChainId, cancellationToken) is null)
            throw new NotFoundException("Chaîne", request.ChainId);

        if (await _options.GetByIdAsync(request.OptionId, cancellationToken) is null)
            throw new NotFoundException("Option", request.OptionId);

        foreach (var charmId in request.Items.Select(i => i.CharmId).Distinct())
        {
            if (await _charms.GetByIdAsync(charmId, cancellationToken) is null)
                throw new NotFoundException("Breloque", charmId);
        }

        var creation = new Creation
        {
            UserId = request.UserId,
            ChainId = request.ChainId,
            OptionId = request.OptionId,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items
                .Select(i => new CreationItem { CharmId = i.CharmId, Position = i.Position })
                .ToList()
        };

        await _creations.AddAsync(creation, cancellationToken);
        await _creations.SaveChangesAsync(cancellationToken);

        // Recharge avec navigations pour un DTO complet
        var full = await _creations.GetByIdWithItemsAsync(creation.Id, cancellationToken);
        return full!.ToDto();
    }
}
