using JewelryShop.Application.Common.Exceptions;
using MediatR;

namespace JewelryShop.Application.Features.Favorites.Commands;

public record RemoveFavoriteCommand(long UserId, long FavoriteId) : IRequest<bool>;

public class RemoveFavoriteCommandHandler : IRequestHandler<RemoveFavoriteCommand, bool>
{
    private readonly IFavoriteRepository _favorites;

    public RemoveFavoriteCommandHandler(IFavoriteRepository favorites) => _favorites = favorites;

    public async Task<bool> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var favorite = await _favorites.GetByIdAsync(request.FavoriteId, cancellationToken)
            ?? throw new NotFoundException("Favori", request.FavoriteId);

        if (favorite.UserId != request.UserId)
            throw new ForbiddenAccessException("Ce favori ne t'appartient pas.");

        _favorites.Delete(favorite);
        await _favorites.SaveChangesAsync(cancellationToken);

        return true;
    }
}
