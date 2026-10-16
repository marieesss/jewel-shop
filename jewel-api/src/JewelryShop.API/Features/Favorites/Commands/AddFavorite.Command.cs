using JewelryShop.Features.Favorites.DTOs;
using MediatR;

namespace JewelryShop.Features.Favorites.Commands;

public record AddFavoriteCommand(long UserId, long? ChainId, long? CharmId) : IRequest<FavoriteDto>;
