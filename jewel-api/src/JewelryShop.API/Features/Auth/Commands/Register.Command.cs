using MediatR;

namespace JewelryShop.Features.Auth.Commands;

public record RegisterCommand(
    string Name,
    string Surname,
    string Email,
    string Password,
    DateOnly? Birthday) : IRequest<AuthResult>;
