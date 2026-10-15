using FluentValidation;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Application.Features.Auth.DTOs;
using MediatR;

namespace JewelryShop.Application.Features.Auth.Commands;

public record RefreshSessionCommand(string? RefreshToken) : IRequest<AuthResult>;

/// <summary>
/// N'impose rien : un token absent n'est pas une erreur de saisie mais une session expirée (401),
/// levée par le handler.
/// </summary>
public class RefreshSessionCommandValidator : AbstractValidator<RefreshSessionCommand>
{
}

public class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, AuthResult>
{
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IJwtService _jwtService;

    public RefreshSessionCommandHandler(IRefreshTokenService refreshTokens, IJwtService jwtService)
    {
        _refreshTokens = refreshTokens;
        _jwtService = jwtService;
    }

    public async Task<AuthResult> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            throw new UnauthorizedException(RefreshTokenService.SessionExpiredMessage);

        var (user, newRefreshToken) = await _refreshTokens.RotateAsync(request.RefreshToken, cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return new AuthResult(new AuthResponse(token, user.Id, user.Role.ToString()), newRefreshToken);
    }
}
