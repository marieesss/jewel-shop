using MediatR;

namespace JewelryShop.Features.Auth.Commands;

public record LogoutCommand(string? RefreshToken) : IRequest;
