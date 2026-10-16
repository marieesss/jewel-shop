using JewelryShop.Features.Auth.DTOs;
using MediatR;

namespace JewelryShop.Features.Auth.Commands;

public record RegisterCommand(
    string Name,
    string Surname,
    string Email,
    string Password,
    DateOnly? Birthday) : IRequest<RegisterResult>;

public record RegisterResult(AuthResponse Response, string RefreshToken) : AuthResult(Response, RefreshToken);
