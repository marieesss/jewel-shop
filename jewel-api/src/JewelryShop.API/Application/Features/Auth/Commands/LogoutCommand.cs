using FluentValidation;
using MediatR;

namespace JewelryShop.Application.Features.Auth.Commands;

public record LogoutCommand(string? RefreshToken) : IRequest;

/// <summary>N'impose rien : se déconnecter sans session active reste un succès.</summary>
public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenService _refreshTokens;

    public LogoutCommandHandler(IRefreshTokenService refreshTokens)
    {
        _refreshTokens = refreshTokens;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return;

        await _refreshTokens.RevokeAsync(request.RefreshToken, cancellationToken);
    }
}
