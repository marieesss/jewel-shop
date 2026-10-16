using MediatR;

namespace JewelryShop.Features.Auth.Commands;

public record ResetPasswordCommand(string Token, string Password) : IRequest;
