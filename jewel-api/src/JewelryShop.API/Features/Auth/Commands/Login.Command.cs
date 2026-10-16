using MediatR;

namespace JewelryShop.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
