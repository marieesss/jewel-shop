using MediatR;

namespace JewelryShop.Features.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest;
