using FluentValidation;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Chains;
using JewelryShop.Application.Features.Charms;
using JewelryShop.Application.Features.Creations.DTOs;
using JewelryShop.Application.Features.Options;
using MediatR;

namespace JewelryShop.Application.Features.Creations.Commands;

public record CreateCreationItemInput(long CharmId, int Position);

public record CreateCreationCommand(
    long UserId,
    long ChainId,
    long OptionId,
    IReadOnlyList<CreateCreationItemInput> Items) : IRequest<CreationDto>;

public class CreateCreationCommandValidator : AbstractValidator<CreateCreationCommand>
{
    public CreateCreationCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ChainId).GreaterThan(0);
        RuleFor(x => x.OptionId).GreaterThan(0);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Une création doit contenir au moins une breloque.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.CharmId).GreaterThan(0);
            item.RuleFor(i => i.Position).GreaterThanOrEqualTo(0);
        });

        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.Position).Distinct().Count() == items.Count)
            .WithMessage("Les positions des breloques doivent être uniques.")
            .When(x => x.Items is { Count: > 0 });
    }
}

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
