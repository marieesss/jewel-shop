using MediatR;

namespace JewelryShop.Features.Favorites.Commands;

public record RemoveFavoriteCommand(long UserId, long FavoriteId) : IRequest<bool>;
