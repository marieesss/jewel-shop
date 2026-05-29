using FluentValidation;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Features.Chains;
using JewelryShop.Application.Features.Charms;
using JewelryShop.Application.Features.Favorites.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Favorites.Commands;

public record AddFavoriteCommand(long UserId, long? ChainId, long? CharmId) : IRequest<FavoriteDto>;

public class AddFavoriteCommandValidator : AbstractValidator<AddFavoriteCommand>
{
    public AddFavoriteCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);

        RuleFor(x => x)
            .Must(x => x.ChainId.HasValue ^ x.CharmId.HasValue)
            .WithMessage("Renseigne exactement un produit : soit chainId, soit charmId (pas les deux, pas aucun).");
    }
}

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
            throw new ConflictException("Ce produit est déjà dans tes favoris.");

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
