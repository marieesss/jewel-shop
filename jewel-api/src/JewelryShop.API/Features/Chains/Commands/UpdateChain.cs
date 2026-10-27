using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Chains.DTOs;
using JewelryShop.Features.Tags.Common;
using MediatR;

namespace JewelryShop.Features.Chains.Commands;

public class UpdateChainCommandHandler : IRequestHandler<UpdateChainCommand, AdminChainDto>
{
    private readonly IChainRepository _chains;
    private readonly IChainLengthRepository _lengths;
    private readonly ITagRepository _tags;

    public UpdateChainCommandHandler(IChainRepository chains, IChainLengthRepository lengths, ITagRepository tags)
    {
        _chains = chains;
        _lengths = lengths;
        _tags = tags;
    }

    public async Task<AdminChainDto> Handle(UpdateChainCommand request, CancellationToken cancellationToken)
    {
        var chain = await _chains.GetByIdWithRollsAndTagsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chaîne", request.Id);

        chain.Name = request.Name.Trim();
        chain.Description = request.Description?.Trim() ?? string.Empty;
        chain.Color = request.Color;
        chain.ApplyPrices(request);
        chain.Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim();

        // Le front en production n'envoie pas tagIds : son absence laisse les tags tels quels.
        if (request.TagIds is { } tagIds)
            chain.ReplaceTags(await _tags.GetForProductAsync(tagIds, nameof(request.TagIds), cancellationToken));

        _chains.Update(chain);
        await _chains.SaveChangesAsync(cancellationToken);

        return chain.ToAdminDto(await _lengths.GetAllOrderedAsync(cancellationToken));
    }
}
