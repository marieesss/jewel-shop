using FluentValidation.Results;
using JewelryShop.Common.Exceptions;

namespace JewelryShop.Features.Orders.Common;

/// <summary>
/// Vérifie un collier contre la base, pour le devis comme pour la commande : chaîne et breloques existantes,
/// longueur proposée, et stock suffisant. N'écrit rien.
/// </summary>
public class NecklaceComposer
{
    private readonly IChainRepository _chains;
    private readonly IChainLengthRepository _lengths;
    private readonly ICharmRepository _charms;

    public NecklaceComposer(IChainRepository chains, IChainLengthRepository lengths, ICharmRepository charms)
    {
        _chains = chains;
        _lengths = lengths;
        _charms = charms;
    }

    public async Task<Necklace> ComposeAsync(INecklaceComposition composition, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdWithRollsAsync(composition.ChainId, cancellationToken)
            ?? throw new NotFoundException(NecklaceMessages.ChainNotFound);

        await EnsureLengthIsOfferedAsync(composition.LengthCm, cancellationToken);

        var charms = await LoadCharmsInOrderAsync(composition.CharmIds, cancellationToken);

        if (chain.PricePerMeter is null)
            throw new ConflictException(NecklaceMessages.NotForSale(chain.Name));

        if (!chain.CanSell(composition.LengthCm))
            throw new ConflictException(NecklaceMessages.OutOfStock(chain.Name));

        EnsureCharmsAreInStock(charms);

        return new Necklace(chain, composition.LengthCm, charms);
    }

    private async Task EnsureLengthIsOfferedAsync(int lengthCm, CancellationToken cancellationToken)
    {
        if (!await _lengths.ExistsByLengthAsync(lengthCm, cancellationToken: cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(INecklaceComposition.LengthCm), NecklaceMessages.LengthNotOffered)]);
    }

    private async Task<IReadOnlyList<Charm>> LoadCharmsInOrderAsync(IReadOnlyList<long> charmIds, CancellationToken cancellationToken)
    {
        var charmsById = (await _charms.GetByIdsAsync(charmIds.Distinct().ToList(), cancellationToken))
            .ToDictionary(c => c.Id);

        return charmIds
            .Select(id => charmsById.GetValueOrDefault(id) ?? throw new NotFoundException(NecklaceMessages.CharmNotFound))
            .ToList();
    }

    private static void EnsureCharmsAreInStock(IReadOnlyList<Charm> charms)
    {
        var shortCharm = charms
            .GroupBy(c => c.Id)
            .FirstOrDefault(copies => copies.Count() > copies.First().Stock);

        if (shortCharm is not null)
            throw new ConflictException(NecklaceMessages.OutOfStock(shortCharm.First().Name));
    }
}
