using JewelryShop.Common.Exceptions;
using JewelryShop.Features.Favorites.DTOs;
using MediatR;

namespace JewelryShop.Features.Favorites.Commands;

public class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, FavoriteDto>
{
    private readonly IFavoriteRepository _favorites;
    private readonly IChainRepository _chains;
    private readonly ICharmRepository _charms;

    public AddFavoriteCommandHandler(
        IFavoriteRepository favorites,
        IChainRepository chains,
        ICharmRepository charms)
    {
        _favorites = favorites;
        _chains = chains;
        _charms = charms;
    }

    public async Task<FavoriteDto> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        Chain? chain = null;
        Charm? charm = null;

        if (request.ChainId.HasValue)
        {
            chain = await _chains.GetByIdAsync(request.ChainId.Value, cancellationToken)
                ?? throw new NotFoundException("Chaîne", request.ChainId.Value);
        }
        else if (request.CharmId.HasValue)
        {
            charm = await _charms.GetByIdAsync(request.CharmId.Value, cancellationToken)
                ?? throw new NotFoundException("Breloque", request.CharmId.Value);
        }

        if (await _favorites.ExistsAsync(request.UserId, request.ChainId, request.CharmId, cancellationToken))
            throw new ConflictException("Ce produit est déjà dans vos favoris.");

        var favorite = new Favorite
        {
            UserId = request.UserId,
            ChainId = request.ChainId,
            CharmId = request.CharmId,
            Chain = chain,
            Charm = charm
        };

        await _favorites.AddAsync(favorite, cancellationToken);
        await _favorites.SaveChangesAsync(cancellationToken);

        return favorite.ToDto();
    }
}
