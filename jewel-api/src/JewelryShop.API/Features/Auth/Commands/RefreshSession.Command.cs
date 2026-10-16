using JewelryShop.Features.Auth.DTOs;
using MediatR;

namespace JewelryShop.Features.Auth.Commands;

public record RefreshSessionCommand(string? RefreshToken) : IRequest<RefreshSessionResult>;

public record RefreshSessionResult(AuthResponse Response, string RefreshToken) : AuthResult(Response, RefreshToken);
