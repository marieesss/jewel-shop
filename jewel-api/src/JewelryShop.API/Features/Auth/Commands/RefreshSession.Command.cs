using MediatR;

namespace JewelryShop.Features.Auth.Commands;

public record RefreshSessionCommand(string? RefreshToken) : IRequest<AuthResult>;
