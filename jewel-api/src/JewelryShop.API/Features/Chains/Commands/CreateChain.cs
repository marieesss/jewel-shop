using JewelryShop.Features.Chains.Common;
using JewelryShop.Features.Chains.DTOs;
using MediatR;

namespace JewelryShop.Features.Chains.Commands;

public class CreateChainCommandHandler : IRequestHandler<CreateChainCommand, ChainDto>
{
    private readonly IChainRepository _chains;
    private readonly IChainLengthRepository _lengths;
    private readonly TimeProvider _timeProvider;

    public CreateChainCommandHandler(IChainRepository chains, IChainLengthRepository lengths, TimeProvider timeProvider)
    {
        _chains = chains;
        _lengths = lengths;
        _timeProvider = timeProvider;
    }

    public async Task<ChainDto> Handle(CreateChainCommand request, CancellationToken cancellationToken)
    {
        var chain = new Chain
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Color = request.Color,
            Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim()
        };
        chain.ApplyPrices(request);

        var rollLengthCm = InitialRollLengthCm(request);
        if (rollLengthCm > 0)
            chain.AddRoll(rollLengthCm, _timeProvider.GetUtcNow().UtcDateTime);

        await _chains.AddAsync(chain, cancellationToken);
        await _chains.SaveChangesAsync(cancellationToken);

        return chain.ToDto(await _lengths.GetAllOrderedAsync(cancellationToken));
    }

    // À l'ancien format, la longueur saisie est celle de l'unique rouleau, comme pour les chaînes
    // reprises par la migration V7.
    private static int InitialRollLengthCm(CreateChainCommand request) =>
        request.UsesMeterPrices()
            ? request.InitialRollLengthCm!.Value
            : ChainPricing.RollLengthCm(request.Length!.Value);
}
