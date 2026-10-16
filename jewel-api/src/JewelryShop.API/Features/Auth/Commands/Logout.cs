using MediatR;

namespace JewelryShop.Features.Auth.Commands;

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
